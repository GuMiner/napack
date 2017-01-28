using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Napack.Common
{
    public class NewNapackMetadata
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
    }
}