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
        /// The hashes of users authorized to modify this package.
        /// </summary>
        public List<string> AuthorizedUserHashes { get; set; }

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
