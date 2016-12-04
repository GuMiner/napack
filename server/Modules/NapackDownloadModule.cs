using System;
using Nancy;
using System.Linq;
using System.Collections.Generic;
using Napack.Common;

namespace Napack.Server.Modules
{
    /// <summary>
    /// Handles Napack Framework Server download operations.
    /// </summary>
    public class NapackDownloadModule : NancyModule
    {
        public NapackDownloadModule(INapackStorageManager napackManager)
            : base("/napackDownload")
        {
            // Downloads a specific Napack package.
            Get["/{fullPackageName}"] = parameters =>
            {
                string fullPackageName = parameters.fullPackageName;
                List<string> components = fullPackageName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (components.Count != 4)
                {
                    // TODO this convention needs to be standardized somewhere.
                    return this.Response.AsJson(new
                    {
                        Error = "An invalid number of version components were provided in the version string!",
                        Message = $"Components provided: {components.Count}"
                    }, HttpStatusCode.BadRequest);
                }

                string packageName = components[0];
                components.RemoveAt(0);

                List<int> versionParts;
                try
                {
                    versionParts = components.Select(item => int.Parse(item)).ToList();
                }
                catch (Exception ex)
                {
                    return this.Response.AsJson(new
                    {
                        Error = "Could not parse the version components provided!",
                        Message = ex.Message
                    }, HttpStatusCode.BadRequest);
                }

                return GetSpecificPackage(napackManager, packageName, versionParts[0], versionParts[1], versionParts[2]);
            };

            // Downloads the most recent (minor/patch) of the specified major-version Napack package.
            Get["/dependency/{partialPackageName}"] = parameters =>
            {
                string partialPackageName = parameters.partialPackageName;
                List<string> components = partialPackageName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (components.Count != 2)
                {
                    // TODO this convention needs to be standardized somewhere.
                    return this.Response.AsJson(new
                    {
                        Error = "An invalid number of version components were provided in the version string!",
                        Message = $"Components provided: {components.Count}"
                    }, HttpStatusCode.BadRequest);
                }

                string packageName = components[0];

                int majorVersion;
                try
                {
                    majorVersion = int.Parse(components[1]);
                }
                catch (Exception ex)
                {
                    return this.Response.AsJson(new
                    {
                        Error = "Could not parse the version components provided!",
                        Message = ex.Message
                    }, HttpStatusCode.BadRequest);
                }

                return GetMostRecentMajorVersion(napackManager, packageName, majorVersion);
            };
        }

        private Response GetMostRecentMajorVersion(INapackStorageManager napackManager, string name, int major)
        {
            int minorVersion = 0;
            return this.GetPackage(napackManager, name, major, (majorVersion) =>
            {
                minorVersion = majorVersion.Versions.Max(version => version.Key);
                return minorVersion;
            },
            (majorVersion) => majorVersion.Versions[minorVersion].Max());
        }

        private Response GetSpecificPackage(INapackStorageManager napackManager, string name, int major, int minor, int patch)
        {
            return this.GetPackage(napackManager, name, major, (unused) => minor, (unused) => patch);
        }

        private Response GetPackage(INapackStorageManager napackManager, string name, int major, 
            Func<NapackMajorVersionMetadata, int> minorVersionComputer, Func<NapackMajorVersionMetadata, int> patchVersionComputer)
        {
            NapackMetadata package = napackManager.GetPackageMetadata(name);

            try
            {
                NapackMajorVersionMetadata majorVersion = package.GetMajorVersion(major);
                if (majorVersion.Recalled)
                {
                    return this.Response.AsJson(new
                    {
                        Error = "The specified major version, " + major + ", has been recalled.",
                        Message = "..."
                    }, HttpStatusCode.NoContent);
                }

                int minor = minorVersionComputer(majorVersion);
                int patch = patchVersionComputer(majorVersion);

                NapackVersion specifiedVersion = napackManager.GetPackageVersion(new NapackVersionIdentifier(name, major, minor, patch));
                Common.NapackVersion downloadableVersion = new Common.NapackVersion(major, minor, patch,
                    specifiedVersion.Authors, specifiedVersion.Files, majorVersion.License, specifiedVersion.Dependencies);
                return this.Response.AsJson(downloadableVersion, HttpStatusCode.OK);
            }
            catch (NapackVersionNotFoundException vnf)
            {
                return this.Response.AsJson(new
                {
                    Error = "The specified napack package or version was not found.",
                    Message = vnf.Message
                }, HttpStatusCode.NotFound);
            }
        }
    }
}
