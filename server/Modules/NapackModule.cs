using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using Nancy;
using Napack.Common;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;
using Napack.Server.Utils;

namespace Napack.Server.Modules
{
    /// <summary>
    /// Handles Napack Framework Server query operations.
    /// </summary>
    public class NapackModule : NancyModule
    {
        public NapackModule()
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
                    NapackMetadata package = Global.NapackStorageManager.GetPackageMetadata(packageName, false);
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
                        NapackMetadata package = Global.NapackStorageManager.GetPackageMetadata(packageName, false);
                        NapackMajorVersionMetadata majorVersion = package.GetMajorVersion(components[0]);

                        return this.Response.AsJson(majorVersion.AsSummaryJson());
                    }
                    else if (components.Count == 3)
                    {
                        NapackVersion specificVersion = Global.NapackStorageManager.GetPackageVersion(new NapackVersionIdentifier(packageName, components[0], components[1], components[2]));
                        Global.NapackStorageManager.IncrementPackageDownload(packageName);

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
                if (Global.NapackStorageManager.ContainsNapack(packageName))
                {
                    throw new DuplicateNapackException();
                }

                // Validate user, name and API.
                NewNapack newNapack = SerializerExtensions.Deserialize<NewNapack>(this.Context);
                newNapack.Validate();

                UserIdentifier.VerifyAuthorization(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), Global.NapackStorageManager, newNapack.metadata.AuthorizedUserIds);
                NapackSpec generatedApiSpec = NapackAnalyst.CreateNapackSpec(packageName, newNapack.NewNapackVersion.Files);
                NapackModule.ValidateDependentPackages(Global.NapackStorageManager, newNapack.NewNapackVersion);

                newNapack.NewNapackVersion.UpdateNamespaceOfFiles(packageName, 1);
                Global.NapackStorageManager.SaveNewNapack(packageName, newNapack, generatedApiSpec);

                return this.Response.AsJson(new
                {
                    Message = "Created package " + packageName
                }, HttpStatusCode.Created);
            };

            // Updates the definition (metadata) of a Napack package.
            Put["/{packageName"] = parameters =>
            {
                string packageName = parameters.packageName;
                NapackMetadata package = Global.NapackStorageManager.GetPackageMetadata(packageName, true);

                NewNapackMetadata metadata = SerializerExtensions.Deserialize<NewNapackMetadata>(this.Context);
                UserIdentifier.VerifyAuthorization(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), Global.NapackStorageManager, metadata.AuthorizedUserIds);

                if (metadata.AuthorizedUserIds.Count == 0)
                {
                    throw new InvalidNapackException("At least one authorized user ID must be provided.");
                }

                package.AuthorizedUserIds = metadata.AuthorizedUserIds;
                package.Description = metadata.Description;
                package.MoreInformation = metadata.MoreInformation;
                package.Tags = metadata.Tags;
                Global.NapackStorageManager.UpdatePackageMetadata(package);

                return this.Response.AsJson(new
                {
                    Message = "Updated package metadata " + packageName
                });
            };

            // Updates an existing Napack package.
            Patch["/{packageName}"] = parameters =>
            {
                NewNapackVersion newNapackVersion = SerializerExtensions.Deserialize<NewNapackVersion>(this.Context);
                newNapackVersion.Validate();

                string packageName = parameters.packageName;
                NapackMetadata package = Global.NapackStorageManager.GetPackageMetadata(packageName, true);
                UserIdentifier.VerifyAuthorization(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), Global.NapackStorageManager, package.AuthorizedUserIds);
                
                // Validate and create a spec for this new version.
                NapackSpec newVersionSpec = NapackAnalyst.CreateNapackSpec(packageName, newNapackVersion.Files);
                NapackModule.ValidateDependentPackages(Global.NapackStorageManager, newNapackVersion);

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
                    NapackSpec oldVersionSpec = Global.NapackStorageManager.GetPackageSpecification(oldVersionId);
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

                newNapackVersion.UpdateNamespaceOfFiles(packageName, upversionType != NapackAnalyst.UpversionType.Major ? majorVersion : majorVersion + 1);
                Global.NapackStorageManager.SaveNewNapackVersion(package, new NapackVersionIdentifier(packageName, majorVersion, minorVersion, patchVersion), upversionType, newNapackVersion, newVersionSpec);

                return this.Response.AsJson(new
                {
                    Message = "Updated package " + packageName,
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
                NapackMetadata package = napackManager.GetPackageMetadata(napackVersion.Name, false);
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
