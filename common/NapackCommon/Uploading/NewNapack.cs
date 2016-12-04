using System;
using System.Collections.Generic;

namespace Napack.Common
{
    /// <summary>
    /// Lists the necessary fields for creating a new Napack.
    /// </summary>
    /// <remarks>
    /// The name is provided in the URI.
    /// </remarks>
    public class NewNapack
    {
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
        /// The new napack to use to create version 1.0.0 of this package.
        /// </summary>
        public NewNapackVersion NewNapackVersion { get; set; }
    }
}