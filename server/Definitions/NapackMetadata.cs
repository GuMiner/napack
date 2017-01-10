using System;
using System.Collections.Generic;
using System.Linq;
using Napack.Common;

namespace Napack.Server
{
    public class NapackMetadata
    {
        /// <summary>
        /// The name of the package.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the package.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The URI containing more information about this package.
        /// </summary>
        public Uri MoreInformation { get; set; }

        /// <summary>
        /// The tags associated with this package.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>   
        /// The IDs of users authorized to modify this package.
        /// </summary>
        public List<string> AuthorizedUserIds { get; set; }

        /// <summary>
        /// Mapping of the available major
        /// </summary>
        public Dictionary<int, NapackMajorVersionMetadata> Versions { get; set; }

       /// <summary>
        /// Gets the specified major version.
        /// </summary>
        /// <exception cref="NapackVersionNotFoundException">If the specified major version was not found.</exception>
        public NapackMajorVersionMetadata GetMajorVersion(int majorVersion)
        {
            if (!this.Versions.ContainsKey(majorVersion))
            {
                throw new NapackVersionNotFoundException(majorVersion);
            }

            return this.Versions[majorVersion];
        }

        public static NapackMetadata CreateFromNewNapack(string napackName, NewNapack newNapack)
        {
            NapackMetadata napackMetadata = new NapackMetadata()
            {
                Name = napackName,
                Description = newNapack.Description,
                MoreInformation = newNapack.MoreInformation,
                Tags = newNapack.Tags ?? new List<string>(),
                AuthorizedUserIds = newNapack.AuthorizedUserIds,
                Versions = new Dictionary<int, NapackMajorVersionMetadata>()
            };

            NapackMajorVersionMetadata majorVersionMetadata = new NapackMajorVersionMetadata()
            {
                Recalled = false,
                License = newNapack.NewNapackVersion.License,
                Versions = new Dictionary<int, List<int>>
                {
                    [0] = new List<int> { 0 }
                }
            };

            napackMetadata.Versions.Add(1, majorVersionMetadata);
            return napackMetadata;
        }

        /// <summary>
        /// Returns this object in summarized format.
        /// </summary>
        /// <returns>A summarized, JSON-serializable dynamic object of this item.</returns>
        public object AsSummaryJson()
        {
            return new
            {
                this.Name,
                this.Description,
                this.MoreInformation,
                this.Tags,
                ValidVersions = this.Versions.Where(version => !version.Value.Recalled).Select(version => version.Key),
                RecalledVersions = this.Versions.Where(version => version.Value.Recalled).Select(version => version.Key)
            };
        }
    }
}
