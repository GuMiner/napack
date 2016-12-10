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

        public void Validate()
        {
            if (this.Description == null)
            {
                throw new InvalidNapackException("The description must be specified.");
            }

            if (this.AuthorizedUserHashes == null || this.AuthorizedUserHashes.Count == 0)
            {
                throw new InvalidNapackException("At least one authorized user hash must be provided.");
            }

            if (this.NewNapackVersion == null)
            {
                throw new InvalidNapackException("The new napack version for this napack must be specified.");
            }

            this.NewNapackVersion.Validate();
        }
    }
}