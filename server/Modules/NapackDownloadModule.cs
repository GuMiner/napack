using System;
using Nancy;
using System.Linq;
using System.Collections.Generic;
using Napack.Common;

namespace Napack.Server.Modules
{
    /// <summary>
    /// Handles Napack Framework Server API operations.
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
            NapackPackage package = napackManager.GetPackage(name);

            try
            {
                NapackMajorVersion majorVersion = package.GetMajorVersion(major);
                if (majorVersion.Recalled)
                {
                    return this.Response.AsJson(new
                    {
                        Error = "The specified major version, " + major + ", has been recalled.",
                        Message = "..."
                    }, HttpStatusCode.NoContent);
                }

                NapackVersion lastMajorVersion = majorVersion.Versions[majorVersion.Versions.Count - 1];
                Common.NapackVersion specificVersion = majorVersion.GetDownloadableVersion(lastMajorVersion.Minor, lastMajorVersion.Patch);
                return this.Response.AsJson(specificVersion, HttpStatusCode.OK);
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

        private Response GetSpecificPackage(INapackStorageManager napackManager, string name, int major, int minor, int patch)
        {
            NapackPackage package = napackManager.GetPackage(name);

            try
            {
                NapackMajorVersion majorVersion = package.GetMajorVersion(major);
                if (majorVersion.Recalled)
                {
                    return this.Response.AsJson(new
                    {
                        Error = "The specified major version, " + major + ", has been recalled.",
                        Message = "..."
                    }, HttpStatusCode.NoContent);
                }

                Common.NapackVersion specificVersion = majorVersion.GetDownloadableVersion(minor, patch);
                return this.Response.AsJson(specificVersion, HttpStatusCode.OK);
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
