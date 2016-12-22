using Napack.Common;
using Newtonsoft.Json;

namespace Napack.Client
{
    public class LicenseLocalDescriptor
    {
        [JsonProperty(Required = Required.Always)]
        public LicenseManagement.LicenseType Type { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string LicenseText { get; set; }

        public void Validate()
        {
            if (!LicenseManagement.IsSupportedLicense(this.Type) && string.IsNullOrWhiteSpace(this.LicenseText))
            {
                throw new InvalidNapackException("If the license is not a supported license, the license text must be specified.");
            }
            else if (LicenseManagement.IsSupportedLicense(this.Type) && !string.IsNullOrWhiteSpace(this.LicenseText))
            {
                throw new InvalidNapackException("If the license is a supported license, the license text must not be specified.");
            }
        }
    }
}