using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Nancy;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Handles Napack Framwork Server API rendering.
    /// </summary>
    public class ApiModule : NancyModule
    {
        public ApiModule(INapackStorageManager napackManager)
            : base("/api")
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
                catch (RuntimeBinderException)
                {
                }

                if (version == null)
                {
                    NapackMetadata metadata = napackManager.GetPackageMetadata(packageName);
                    return View["NapackVersions", new VersionsModel(metadata)];
                }
                else
                {
                    // Attempt to parse our the version string.
                    List<int> components;
                    try
                    {
                        components = version.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(item => int.Parse(item)).ToList();
                        if (components.Count != 3)
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        throw new InvalidNapackVersionException();
                    }

                    NapackVersionIdentifier versionId = new NapackVersionIdentifier(packageName, components[0], components[1], components[2]);
                    NapackSpec spec = napackManager.GetPackageSpecification(versionId);
                    NapackVersion packageVersion = napackManager.GetPackageVersion(versionId);

                    return View["NapackApi", new ApiModel(versionId.GetFullName(), spec, packageVersion.Dependencies)];
                }
            };
        }
    }
}
