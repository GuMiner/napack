using System;

namespace Napack.Common
{
    public class License
    {
        /// <summary>
        /// The name of the license.
        /// </summary>
        public LicenseManagement.LicenseType LicenseType { get; set; }

        /// <summary>
        /// Returns true if this is a supported license.
        /// </summary>
        public bool IsSupportedLicense { get; set; }

        /// <summary>
        /// Returns true if this is a copy-left license.
        /// </summary>
        public bool IsCopyLeft { get; set; }

        /// <summary>
        /// Returns true if this is a commercial license.
        /// </summary>
        public bool IsCommercial { get; set; }

        /// <summary>
        /// Returns true if this is a custom license.
        /// </summary>
        public bool IsCustomLicense { get; set; }

        /// <summary>
        /// If this is a custom license, returns the license text.
        /// If not, returns a user-friendly reference to retrieving the text.
        /// </summary>
        public string LicenseText { get; set; }

        public object AsSummaryJson()
        {
            return new
            {
                Type = this.LicenseType,
                this.IsSupportedLicense,
                this.IsCopyLeft,
                this.IsCommercial,
                this.IsCustomLicense,
            };
        }

        public void VerifyCompatibility(string napackName, int version, License license)
        {
            if (license.IsSupportedLicense && (this.IsSupportedLicense || this.IsCopyLeft))
            {
                // This is a supported license or a copy-left license.
                return;
            }
            else if (license.IsCopyLeft && this.IsCopyLeft)
            {
                // Both are copy-left, likely ok.
                return;
            }
            else if ((license.IsCommercial || license.IsCustomLicense) && (this.IsCommercial || this.IsCustomLicense))
            {
                // Both are commercial/custom, likely ok.
                return;
            }

            // Note that this does not catch *all* incompatible cases, especially when delving into the realm of commercial or copy=left.
            throw new InvalidNapackException("The package " + napackName + "." + version + " has an incompatible license with the provided license.");
        }

        public bool NeedsMajorUpversioning(License license)
        {
            return (this.LicenseType != license.LicenseType ||
                    this.IsSupportedLicense != license.IsSupportedLicense ||
                    this.IsCopyLeft != license.IsCopyLeft ||
                    this.IsCommercial != license.IsCommercial ||
                    this.IsCustomLicense != license.IsCommercial ||
                    !this.LicenseText.Equals(license.LicenseText, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}