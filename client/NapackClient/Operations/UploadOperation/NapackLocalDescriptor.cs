using System;
using System.Collections.Generic;
using System.Linq;
using Napack.Common;

namespace Napack.Client
{
    /// <summary>
    /// Defines a napack as stored on the local disk.
    /// </summary>
    internal class NapackLocalDescriptor
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Uri MoreInformation { get; set; }

        public List<string> Tags { get; set; }

        public List<string> AuthorizedUserHashes { get; set; }

        public List<string> Authors { get; set; }

        public LicenseLocalDescriptor License { get; set; }

        /// <summary>
        /// Dependent napacks for this napack. 
        /// [Key: Napack name] -> [Value: Napack major version]
        /// </summary>
        public Dictionary<string, int> Dependencies { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name) || string.IsNullOrWhiteSpace(this.Description))
            {
                throw new InvalidNapackException("The name and description fields must be populated.");
            }

            if (this.MoreInformation == null || this.Authors == null)
            {
                throw new InvalidNapackException("The more information URI and authors list must be present, even if one or both are not populated.");
            }

            if (!this.Tags.Any(tag => !string.IsNullOrWhiteSpace(tag)) || !this.AuthorizedUserHashes.Any(user => !string.IsNullOrWhiteSpace(user)))
            {
                throw new InvalidNapackException("The tags and authorized user hashes fields must both contain at least one populated value.");
            }

            this.License.Validate();
        }
    }
}