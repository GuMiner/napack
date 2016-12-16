using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace Napack.Analyst
{
    /// <summary>
    /// Describes how to analyze Napacks to determine their public API and properly enforce semantic versioning.
    /// </summary>
    /// <remarks>
    /// The "Public API" in Napack terms is anything a consumer could use to reference the napack, without using C# reflection.
    /// Additional details on the restrictions imposed by the Napack system are listed in the documentation and will be called out in the function signatures below.
    /// 
    /// A naive implementation of this analyst could compile the provided Napack files into an in-memory DLL and use C# reflection itself to validate the Napack.
    /// However, the naive implementation has several blocking problems:
    /// - What default C# assemblies should be included in the compilation process?
    /// - How many dependent Napacks are required (hint -- may be several) to compile the provided Napacks?
    /// - How do we detect files that aren't scheduled for compilation?
    /// - How easy is it to reflect compilation failures to the end-user? How secure is running the compilation process server-side?
    /// 
    /// Another implementation would be to write a custom parser/lexer as per the C# Language Specifications (https://www.microsoft.com/en-us/download/details.aspx?id=7029), 
    ///  instead of relying on C# reflection or compilation. While conceptually simple, implementing that idea is quite a lot of work and effectively duplicates the Roslyn API!
    /// 
    /// This implementation uses the Roslyn API to analyze Napack files *without* compilation to focus on providing meaningful Napack restrictions without reimplementing C# parsing.
    /// </remarks>
    public static class NapackAnalyst
    {
        private static PackageValidationConfig PackageValidationConfig = null;

        public enum UpversionType
        {
            Major = 0,
            Minor = 1,
            Patch = 2
        }

        /// <summary>
        /// Initializes the static configuration for this analyzer.
        /// </summary>
        public static void Initialize()
        {
            string configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PackageValidation.json");
            Console.WriteLine(configFilePath);

            NapackAnalyst.PackageValidationConfig = Serializer.Deserialize<PackageValidationConfig>(File.ReadAllText(configFilePath));
        }

        /// <summary>
        /// Creates a Napack spec for the defined napack files.
        /// </summary>
        /// <param name="napackName">The Napack package name.</param>
        /// <param name="napackFiles">A mapping of file path + name -> file itself.</param>
        /// <remarks>
        /// Restrictions:
        /// - All Napack files with MSBuild type <see cref="NapackFile.ContentType"/> must be able to be analyzed by this system.
        /// - The namespace of all analyzable-files must be the specified napack name (CASE-SENSITIVE, as per C# spec).
        /// - Although C# allows multiple namespaces in a single file, as they all must be the specified napack name doing so is prohibited.
        /// - Partial classes are disallowed as any non-code files won't have their namespace updated. (This may be changed to better support UI elements, etc in the future).
        /// </remarks>
        /// <exception cref="InvalidNapackFileExtensionException">If a Napack file contains a prohibited extension.</exception>
        /// <exception cref="InvalidNamespaceException">If a compilable Napack file is in the wrong namespace.</exception>
        /// <exception cref="InvalidNapackFileException">If a Napack file is listed with MSBuild type <see cref="NapackFile.ContentType"/>, but could not be analyzed.</exception>
        /// <exception cref="UnsupportedNapackFileException">If a Napack file uses C# functionality or syntax that the Napack system explicitly prohibits.</exception>
        public static NapackSpec CreateNapackSpec(string napackName, IDictionary<string, NapackFile> napackFiles)
        {
            NapackSpec spec = new NapackSpec();
            foreach (KeyValuePair<string, NapackFile> fileEntry in napackFiles)
            {
                // Validate the extension.
                string filename = Path.GetFileName(fileEntry.Key);
                string extension = Path.GetExtension(fileEntry.Key);
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    NapackAnalyst.PackageValidationConfig.ValidateExtension(filename, extension);
                }

                if (fileEntry.Value.MsbuildType.Equals(NapackFile.ContentType, StringComparison.InvariantCultureIgnoreCase))
                {
                    spec.Classes.AddRange(NapackClassAnalyzer.Analyze(napackName, filename, fileEntry.Value.Contents));
                }
                else
                {
                    spec.UnknownFiles.Add(fileEntry.Value);
                }
            }

            return spec;
        }

        /// <summary>
        /// Determines the requried upversioning when code transitions from the old Napack to the new Napack.
        /// </summary>
        /// <remarks>
        /// Major == breaking changes.
        /// Minor == publically-facing API was added (but not changed or removed) from the new Napack.
        /// Patch == The publically-facing API is identical between both Napacks, excluding documentation.
        /// </remarks>
        public static UpversionType DeterminedRequiredUpversioning(NapackSpec oldNapack, NapackSpec newNapack)
        {
            UpversionType maxUpversionType = UpversionType.Patch;
            foreach (ClassSpec oldClassSpec in oldNapack.Classes)
            {
                ClassSpec newClassSpec = newNapack.Classes
                    .FirstOrDefault(cls => cls.Name.Name.Equals(oldClassSpec.Name.Name, StringComparison.InvariantCulture));
                UpversionType upversionType = NapackAnalyst.AnalyzeClass(oldClassSpec, newClassSpec);
                if (upversionType == UpversionType.Major)
                {
                    // Exit early, as we found a breaking change.
                    return UpversionType.Major;
                }
                else if (upversionType == UpversionType.Minor)
                {
                    maxUpversionType = UpversionType.Minor;
                }
            }

            // No APIs added in the prior classes. Did the new package add classes?
            if (maxUpversionType == UpversionType.Patch && newNapack.Classes.Count != oldNapack.Classes.Count)
            {
                maxUpversionType = UpversionType.Minor;
            }

            return maxUpversionType;
        }

        private static UpversionType AnalyzeClass(ClassSpec oldClassSpec, ClassSpec newClassSpec)
        {
            UpversionType maxUpversionType = UpversionType.Patch;
            if (newClassSpec == null)
            {
                // The new spec removed a publically facing class.
                return UpversionType.Major;
            }

            // Check if any modifier changes force a major (breaking) change.
            if ((!oldClassSpec.IsAbstract && newClassSpec.IsAbstract) ||
                (oldClassSpec.IsStatic != newClassSpec.IsStatic) ||
                (!oldClassSpec.IsSealed && newClassSpec.IsSealed))
            {
                return UpversionType.Major;
            }

            // Verify all old fields exist
            UpversionType upversionType = NapackAnalyst.AnalyzeFields(oldClassSpec.PublicFields, newClassSpec.PublicFields);
            if (upversionType == UpversionType.Major)
            {
                return upversionType;
            }
            else if (upversionType == UpversionType.Minor)
            {
                maxUpversionType = UpversionType.Minor;
            }

            upversionType = NapackAnalyst.AnalyzeProperties(oldClassSpec.PublicProperties, newClassSpec.PublicProperties);
            if (upversionType == UpversionType.Major)
            {
                return upversionType;
            }
            else if (upversionType == UpversionType.Minor)
            {
                maxUpversionType = UpversionType.Minor;
            }

            upversionType = NapackAnalyst.AnalyzeConstructors(oldClassSpec.PublicConstructors, newClassSpec.PublicConstructors);
            if (upversionType == UpversionType.Major)
            {
                return upversionType;
            }
            else if (upversionType == UpversionType.Minor)
            {
                maxUpversionType = UpversionType.Minor;
            }

            upversionType = NapackAnalyst.AnalyzeMethods(oldClassSpec.PublicMethods, newClassSpec.PublicMethods);
            if (upversionType == UpversionType.Major)
            {
                return upversionType;
            }
            else if (upversionType == UpversionType.Minor)
            {
                maxUpversionType = UpversionType.Minor;
            }

            // Analyze classes recursively.
            foreach (ClassSpec oldClass in oldClassSpec.PublicClasses)
            {
                ClassSpec newClass = newClassSpec.PublicClasses
                    .FirstOrDefault(cls => cls.Name.Name.Equals(oldClass.Name.Name, StringComparison.InvariantCulture));
                upversionType = NapackAnalyst.AnalyzeClass(oldClass, newClass);
                if (upversionType == UpversionType.Major)
                {
                    return upversionType;
                }
                else if (upversionType == UpversionType.Minor)
                {
                    maxUpversionType = UpversionType.Minor;
                }
            }

            if (maxUpversionType == UpversionType.Patch && oldClassSpec.PublicClasses.Count != newClassSpec.PublicClasses.Count)
            {
                maxUpversionType = UpversionType.Minor;
            }

            return maxUpversionType;
        }

        private static UpversionType AnalyzeFields(IList<FieldSpec> oldFields, IList<FieldSpec> newFields)
        {
            foreach (FieldSpec oldField in oldFields)
            {
                FieldSpec newField = newFields
                    .FirstOrDefault(field => field.Name.Name.Equals(oldField.Name.Name, StringComparison.InvariantCulture));
                if (newField == null)
                {
                    return UpversionType.Major;
                }

                // Verify types are equal. This will false-positive on string/String conversions, but that's a minor issue for later consideration.
                if (!oldField.Type.Equals(newField.Type, StringComparison.InvariantCulture))
                {
                    return UpversionType.Major;
                }

                // Validate modifiers.
                if ((oldField.IsUserModifiable && !newField.IsUserModifiable) ||
                    (oldField.IsStatic != newField.IsStatic))
                {
                    return UpversionType.Major;
                }
            }

            if (newFields.Count != oldFields.Count)
            {
                return UpversionType.Minor;
            }

            return UpversionType.Patch;
        }

        private static UpversionType AnalyzeProperties(IList<PropertySpec> oldProperties, IList<PropertySpec> newProperties)
        {
            foreach (PropertySpec oldProperty in oldProperties)
            {
                PropertySpec newProperty = newProperties
                    .FirstOrDefault(prop => prop.Name.Name.Equals(oldProperty.Name.Name, StringComparison.InvariantCulture));
                if (newProperty == null)
                {
                    return UpversionType.Major;
                }

                // Verify types are equal. This will false-positive on string/String conversions, but that's a minor issue for later consideration.
                if (!oldProperty.Type.Equals(newProperty.Type, StringComparison.InvariantCulture))
                {
                    return UpversionType.Major;
                }

                // Validate modifier
                if (oldProperty.IsStatic != newProperty.IsStatic)
                {
                    return UpversionType.Major;
                }
            }

            if (newProperties.Count != oldProperties.Count)
            {
                return UpversionType.Minor;
            }

            return UpversionType.Patch;
        }

        private static UpversionType AnalyzeConstructors(IList<ConstructorSpec> oldConstructors, IList<ConstructorSpec> newConstructors)
        {
            UpversionType maxUpversionType = UpversionType.Patch;
            foreach (ConstructorSpec oldConstructor in oldConstructors)
            {
                // Constructor matching is complicated as all constructors have the same name, but different parameters.
                // For now, perform an inefficient n^2 matching by comparing each old constructor against each new one.
                bool anyWithoutMajorChanges = false;
                foreach (ConstructorSpec newConstructor in newConstructors)
                {
                    if (!NapackAnalyst.AnayzeParameters(oldConstructor.Parameters, newConstructor.Parameters))
                    {
                        anyWithoutMajorChanges = true;
                        break;
                    }
                }

                if (!anyWithoutMajorChanges)
                {
                    return UpversionType.Major;
                }
            }

            if (maxUpversionType == UpversionType.Patch && newConstructors.Count != oldConstructors.Count)
            {
                return UpversionType.Minor;
            }

            return maxUpversionType;
        }

        private static UpversionType AnalyzeMethods(IList<MethodSpec> oldMethods, IList<MethodSpec> newMethods)
        {
            UpversionType maxUpversionType = UpversionType.Patch;
            foreach (MethodSpec oldMethod in oldMethods)
            {
                MethodSpec newMethod = newMethods
                    .FirstOrDefault(field => field.Name.Name.Equals(oldMethod.Name.Name, StringComparison.InvariantCulture));
                if (newMethod == null)
                {
                    return UpversionType.Major;
                }
                
                if (!newMethod.ReturnType.Equals(oldMethod.ReturnType, StringComparison.InvariantCulture))
                {
                    return UpversionType.Major;
                }

                // Check parameters
                bool isMajorChange = NapackAnalyst.AnayzeParameters(oldMethod.Parameters, newMethod.Parameters);
                if (isMajorChange)
                {
                    return UpversionType.Major;
                }
            }

            if (maxUpversionType == UpversionType.Patch && newMethods.Count != oldMethods.Count)
            {
                return UpversionType.Minor;
            }

            return maxUpversionType;
        }

        /// <summary>
        /// Returns true on a breaking change (MAJOR), false otherwise (PATCH)
        /// </summary>
        private static bool AnayzeParameters(List<ParameterSpec> oldParameters, List<ParameterSpec> newParameters)
        {
            if (newParameters.Count != oldParameters.Count)
            {
                return true;
            }

            // If parameter ordering changes, that's a breaking change.
            for (int i = 0; i < oldParameters.Count; i++)
            {
                if (!oldParameters[i].Name.Equals(newParameters[i].Name, StringComparison.InvariantCulture) ||
                    !oldParameters[i].Type.Equals(newParameters[i].Type, StringComparison.InvariantCulture) ||
                    !oldParameters[i].Modifier.Equals(newParameters[i].Modifier, StringComparison.InvariantCulture) ||
                    !oldParameters[i].Default.Equals(newParameters[i].Default, StringComparison.InvariantCulture))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
