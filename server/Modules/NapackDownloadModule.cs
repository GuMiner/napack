using System;
using System.Linq;
using Nancy;
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
                NapackVersionIdentifier napackId = new NapackVersionIdentifier(fullPackageName);
                return GetSpecificPackage(napackManager, napackId);
            };

            // Downloads the most recent (minor/patch) of the specified major-version Napack package.
            Get["/dependency/{partialPackageName}"] = parameters =>
            {
                string partialPackageName = parameters.partialPackageName;
                NapackVersionIdentifier napackId = new NapackVersionIdentifier(partialPackageName + ".0.0");
                return GetMostRecentMajorVersion(napackManager, napackId.NapackName, napackId.Major);
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

        private Response GetSpecificPackage(INapackStorageManager napackManager, NapackVersionIdentifier napackId)
        {
            return this.GetPackage(napackManager, napackId.NapackName, napackId.Major, (unused) => napackId.Minor, (unused) => napackId.Patch);
        }

        private Response GetPackage(INapackStorageManager napackManager, string name, int major, 
            Func<NapackMajorVersionMetadata, int> minorVersionComputer, Func<NapackMajorVersionMetadata, int> patchVersionComputer)
        {
            NapackMetadata package = napackManager.GetPackageMetadata(name);
            NapackMajorVersionMetadata majorVersion = package.GetMajorVersion(major);
            if (majorVersion.Recalled)
            {
                throw new NapackRecalledException(name, major);
            }

            int minor = minorVersionComputer(majorVersion);
            int patch = patchVersionComputer(majorVersion);

            NapackVersion specifiedVersion = napackManager.GetPackageVersion(new NapackVersionIdentifier(name, major, minor, patch));
            Common.NapackVersion downloadableVersion = new Common.NapackVersion(major, minor, patch,
                specifiedVersion.Authors, specifiedVersion.Files, majorVersion.License, specifiedVersion.Dependencies);
            return this.Response.AsJson(downloadableVersion, HttpStatusCode.OK);
        }
    }
}
