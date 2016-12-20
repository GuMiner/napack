using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using Nancy;
using Napack.Common;
using Nancy.ModelBinding;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;

namespace Napack.Server.Modules
{
    /// <summary>
    /// Handles Napack Framework Server query operations.
    /// </summary>
    public class NapackModule : NancyModule
    {
        public NapackModule(INapackStorageManager napackManager)
            : base("/napacks")
        {
            // Gets a Napack package or series of package versions.
            Get["/{packageName}/{version?}"] = parameters =>
            {
                string packageName = parameters.packageName;
                string version = null;
                try
                {
                    version = parameters.version;
                }
                catch(RuntimeBinderException)
                {
                }

                if (version == null)
                {
                    // The user is asking for all major versions of the specified package.
                    NapackMetadata package = napackManager.GetPackageMetadata(packageName);
                    return this.Response.AsJson(package.AsSummaryJson());
                }
                else
                {
                    // Attempt to parse our the version string.
                    List<int> components;
                    try
                    {
                        components = version.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(item => int.Parse(item)).ToList();
                    }
                    catch (Exception)
                    {
                        throw new InvalidNapackVersionException();
                    }

                    // Handle the resulting version components.
                    if (components.Count == 1 || components.Count == 2)
                    {
                        NapackMetadata package = napackManager.GetPackageMetadata(packageName);
                        NapackMajorVersionMetadata majorVersion = package.GetMajorVersion(components[0]);
                        return this.Response.AsJson(majorVersion.AsSummaryJson());
                    }
                    else if (components.Count == 3)
                    {
                        NapackVersion specificVersion = napackManager.GetPackageVersion(new NapackVersionIdentifier(packageName, components[0], components[1], components[2]));
                        return this.Response.AsJson(specificVersion.AsSummaryJson());
                    }
                    else
                    {
                        throw new InvalidNapackVersionException();
                    }
                }
            };

            // Creates a new Napack package.
            Post["/{packageName}"] = parameters =>
            {
                string packageName = parameters.packageName;
                if (napackManager.ContainsNapack(packageName))
                {
                    throw new DuplicateNapackException();
                }

                // Validate user, name and API.
                NewNapack newNapack = this.Bind<NewNapack>();
                newNapack.Validate();

                UserIdentifier.Validate(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), napackManager, newNapack.AuthorizedUserIds);
                NapackNameValidator.Validate(packageName);
                NapackSpec generatedApiSpec = NapackAnalyst.CreateNapackSpec(packageName, newNapack.NewNapackVersion.Files);
                NapackModule.ValidateDependentPackages(napackManager, newNapack.NewNapackVersion);

                napackManager.SaveNewNapack(packageName, newNapack, generatedApiSpec);

                return this.Response.AsJson(new
                {
                    Message = "Created package " + packageName
                }, HttpStatusCode.Created);
            };

            // Updates an existing Napack package.
            Patch["/{packageName}"] = parameters =>
            {
                NewNapackVersion newNapackVersion = this.Bind<NewNapackVersion>();
                newNapackVersion.Validate();

                string packageName = parameters.packageName;
                NapackMetadata package = napackManager.GetPackageMetadata(packageName);
                UserIdentifier.Validate(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), napackManager, package.AuthorizedUserIds);
                
                // Validate and create a spec for this new version.
                NapackSpec newVersionSpec = NapackAnalyst.CreateNapackSpec(packageName, newNapackVersion.Files);
                NapackModule.ValidateDependentPackages(napackManager, newNapackVersion);

                // Determine what upversioning will be performed.
                int majorVersion = package.Versions.Max(version => version.Key);
                int minorVersion = package.Versions[majorVersion].Versions.Max(version => version.Key);
                int patchVersion = package.Versions[majorVersion].Versions[minorVersion].Max();

                NapackAnalyst.UpversionType upversionType = NapackAnalyst.UpversionType.Patch;
                if (newNapackVersion.ForceMajorUpversioning)
                {
                    // Skip analysis as we know we must go to a new major version.
                    upversionType = NapackAnalyst.UpversionType.Major;
                }
                else
                {
                    // Perform specification and license analysis.
                    NapackVersionIdentifier oldVersionId = new NapackVersionIdentifier(packageName, majorVersion, minorVersion, patchVersion);
                    NapackSpec oldVersionSpec = napackManager.GetPackageSpecification(oldVersionId);
                    NapackAnalyst.UpversionType specUpversionType = NapackAnalyst.DeterminedRequiredUpversioning(oldVersionSpec, newVersionSpec);
                    if (specUpversionType == NapackAnalyst.UpversionType.Major || newNapackVersion.License.NeedsMajorUpversioning(package.GetMajorVersion(majorVersion).License)) 
                    {
                        upversionType = NapackAnalyst.UpversionType.Major;
                    }
                }

                if (upversionType == NapackAnalyst.UpversionType.Patch && newNapackVersion.ForceMinorUpversioning)
                {
                    upversionType = NapackAnalyst.UpversionType.Minor;
                }
                
                napackManager.SaveNewNapackVersion(package, new NapackVersionIdentifier(packageName, majorVersion, minorVersion, patchVersion), upversionType, newNapackVersion, newVersionSpec);

                return this.Response.AsJson(new
                {
                    Message = "Update package " + packageName,
                    Major = majorVersion,
                    Minor = minorVersion,
                    Patch = patchVersion
                });
            };
        }

        private static void ValidateDependentPackages(INapackStorageManager napackManager, NewNapackVersion newNapack)
        {
            // Validate dependent packages exist, aren't recalled, and have valid licenses.
            foreach (NapackMajorVersion napackVersion in newNapack.Dependencies)
            {
                NapackMetadata package = napackManager.GetPackageMetadata(napackVersion.Name);
                NapackMajorVersionMetadata majorVersionMetadata = package.GetMajorVersion(napackVersion.Major);
                if (majorVersionMetadata.Recalled)
                {
                    // Users creating a new napack cannot use recalled packages as they have no reasonable chance of retrieving them.
                    throw new NapackRecalledException(napackVersion.Name, napackVersion.Major);
                }

                newNapack.License.VerifyCompatibility(napackVersion.Name, napackVersion.Major, majorVersionMetadata.License);
            }
        }
    }
}
