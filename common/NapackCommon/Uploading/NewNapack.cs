using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        [JsonProperty(Required = Required.Always)]
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
        [JsonProperty(Required = Required.Always)]
        public List<string> AuthorizedUserIds { get; set; }

        /// <summary>
        /// The new napack to use to create version 1.0.0 of this package.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public NewNapackVersion NewNapackVersion { get; set; }

        public void Validate()
        {
            if (this.AuthorizedUserIds.Count == 0)
            {
                throw new InvalidNapackException("At least one authorized user ID must be provided.");
            }

            this.NewNapackVersion.Validate();
        }
    }
}