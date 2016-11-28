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
            // Gets a downloadable Napack package
            Get["/{fullPackageName}"] = parameters =>
            {
                Global.Log("");
                
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
                
                NapackPackage package = napackManager.GetPackage(packageName);

                try
                {
                    NapackMajorVersion majorVersion = package.GetMajorVersion(versionParts[0]);
                    if (majorVersion.Recalled)
                    {
                        return this.Response.AsJson(new
                        {
                            Error = "The specified major version, " + versionParts[0] + ", has been recalled.",
                            Message = "..."
                        }, HttpStatusCode.NoContent);
                    }

                    Common.NapackVersion specificVersion = majorVersion.GetDownloadableVersion(versionParts[1], versionParts[2]);
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
            };
        }
    }
}
