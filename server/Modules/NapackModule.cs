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
                        NapackVersion specificVersion = napackManager.GetPackageVersion(new Common.NapackVersionIdentifier(packageName, components[0], components[1], components[2]));
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
                
                NewNapack newNapack = this.Bind<NewNapack>();
                UserIdentifier.Validate(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), newNapack.AuthorizedUserHashes);
                NapackNameValidator.Validate(packageName);

                NapackSpec generatedApiSpec = NapackAnalyst.CreateNapackSpec(packageName, newNapack.NewNapackVersion.Files);

                // TODO save, because at this point all our validation has passed.

                return this.Response.AsJson(new
                {
                    Message = "Created package " + packageName
                }, HttpStatusCode.Created);
            };

            // Updates an existing Napack package.
            Patch["/{packageName}"] = parameters =>
            {
                NewNapackVersion newNapackVersion = this.Bind<NewNapackVersion>();

                
                string packageName = parameters.packageName;
                NapackMetadata package = napackManager.GetPackageMetadata(packageName);
                UserIdentifier.Validate(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), package.AuthorizedUserHashes);

                // TODO perform cross-validation to determine how the napack will be updated.

                return this.Response.AsJson(new
                {
                    Message = "Update package " + packageName,
                    // TODO log major, minor, patch versions here.
                });
            };
        }
    }
}
