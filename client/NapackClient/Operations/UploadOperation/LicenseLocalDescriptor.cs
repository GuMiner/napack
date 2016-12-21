using Napack.Common;

namespace Napack.Client
{
    public class LicenseLocalDescriptor
    {
        public LicenseManagement.LicenseType Type { get; set; }
        
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