using Napack.Server.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Napack.Server
{
    public class NapackPackage
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
        /// The users authorized to modify this package.
        /// </summary>
        public List<UserIdentifier> AuthorizedUsers { get; set; }

        /// <summary>
        /// A package version.
        /// </summary>
        /// <remarks>
        /// Package version major IDs are the same as the element index.
        /// </remarks>
        public List<NapackMajorVersion> Versions { get; set; }

        /// <summary>
        /// Gets the specified major version.
        /// </summary>
        /// <param name="majorVersion">The major version to get.</param>
        /// <returns></returns>
        public NapackMajorVersion GetMajorVersion(int majorVersion)
        {
            if (majorVersion < 1 || majorVersion > this.Versions.Count)
            {
                throw new NapackVersionNotFoundException(majorVersion);
            }

            return this.Versions[majorVersion--];
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
                ValidVersions = this.Versions.Where(version => !version.Recalled).Select(version => version.Major),
                RecalledVersions = this.Versions.Where(version => version.Recalled).Select(version => version.Major)
            };
        }
    }
}
