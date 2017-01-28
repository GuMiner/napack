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
        /// Metadata associated with the package, common across all versions.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public NewNapackMetadata metadata { get; set; }

        /// <summary>
        /// The new napack to use to create version 1.0.0 of this package.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public NewNapackVersion NewNapackVersion { get; set; }

        public void Validate()
        {
            if (this.metadata.AuthorizedUserIds.Count == 0)
            {
                throw new InvalidNapackException("At least one authorized user ID must be provided.");
            }

            this.NewNapackVersion.Validate();
        }
    }
}