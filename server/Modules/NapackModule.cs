using System;
using Nancy;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq;
using System.Collections.Generic;

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
                Global.Log("");
                
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
                    catch (Exception ex)
                    {
                        // Ideally I'd separate out exceptions, but the end response is the same so there's no real point.
                        return this.Response.AsJson(new
                        {
                            Error = "Could not parse the version string provided!",
                            Message = ex.Message
                        });
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
                        NapackVersion specificVersion = napackManager.GetPackageVersion(new Common.NapackVersionIdentifier(packageName, components[0], components[1], components[2]));
                        return this.Response.AsJson(specificVersion.AsSummaryJson());
                    }
                    else
                    {
                        return this.Response.AsJson(new { Error = "An invalid number of version components was provided in the version string!", Message = $"Components provided: {components.Count}" });
                    }
                }
            };
        }
    }
}
