using System;
using Nancy;
using Nancy.Extensions;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq;
using System.Collections.Generic;

namespace Napack.Server.Modules
{
    /// <summary>
    /// Handles Napack Framework Server API operations.
    /// </summary>
    public class NapackModule : NancyModule
    {
        // TODO use Nancy IOC to fill this in.
        private static INapackStorageManater NapackManager;

        public NapackModule()
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
                    NapackPackage package = NapackManager.GetPackage(packageName);
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
                    NapackPackage package = NapackManager.GetPackage(packageName);
                    NapackMajorVersion majorVersion = package.GetMajorVersion(components[0]);
                    if (components.Count == 1)
                    {
                        return this.Response.AsJson(majorVersion.AsSummaryJson(null));
                    }
                    else if (components.Count == 2)
                    {
                        return this.Response.AsJson(majorVersion.AsSummaryJson(components[1]));
                    }
                    else if (components.Count == 3)
                    {
                        NapackVersion specificVersion = majorVersion.GetVersion(components[1], components[2]);
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
