using System;
using System.Collections.Generic;
using System.IO;
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
            NapackAnalyst.PackageValidationConfig = Serializer.Deserialize<PackageValidationConfig>(configFilePath);
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
            throw new NotImplementedException();
        }
    }
}
