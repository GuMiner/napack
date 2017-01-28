using System;
using System.Collections.Generic;
using System.Linq;
using Napack.Common;
using Newtonsoft.Json;

namespace Napack.Client
{
    /// <summary>
    /// Defines a napack as stored on the local disk.
    /// </summary>
    internal class NapackLocalDescriptor
    {
        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Uri MoreInformation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<string> Tags { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<string> AuthorizedUserIds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<string> Authors { get; set; }

        [JsonProperty(Required = Required.Always)]
        public LicenseLocalDescriptor License { get; set; }

        /// <summary>
        /// Dependent napacks for this napack. 
        /// [Key: Napack name] -> [Value: Napack major version]
        /// </summary>
        public Dictionary<string, int> Dependencies { get; set; }
        
        public void Validate(string userId, bool validateLicense)
        {
            if (string.IsNullOrWhiteSpace(this.Description))
            {
                throw new InvalidNapackException("The description field must be populated.");
            }

            if (!this.Tags.Any(tag => !string.IsNullOrWhiteSpace(tag)))
            {
                throw new InvalidNapackException("The tags must contain at least one populated value.");
            }

            if (!this.AuthorizedUserIds.Any() && string.IsNullOrWhiteSpace(userId))
            {
                throw new InvalidOperationException("Cannot upload a Napack without authorized users when the default user is not specified in the settings file.");
            }
            else if (!this.AuthorizedUserIds.Contains(userId))
            {
                // If the current user isn't on the authorization list, we add them there.
                this.AuthorizedUserIds.Add(userId);
            }

            this.License.Validate();
        }
    }
}