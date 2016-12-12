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
        /// If this is a copyleft/commercial/custom license, returns the license text.
        /// If not, *this field is unused*
        /// </summary>
        public string LicenseText { get; set; }

        public object AsSummaryJson()
        {
            return new
            {
                Type = this.LicenseType,
                CustomText = this.LicenseText
            };
        }

        public void VerifyCompatibility(string napackName, int version, License license)
        {
            if (LicenseManagement.IsSupportedLicense(license.LicenseType) && 
                (LicenseManagement.IsSupportedLicense(this.LicenseType) || this.LicenseType == LicenseManagement.LicenseType.CopyLeft))
            {
                // Supported or copy-left licenses can consume supported licenses.
                return;
            }
            else if (license.LicenseType == LicenseManagement.LicenseType.CopyLeft && this.LicenseType == LicenseManagement.LicenseType.CopyLeft)
            {
                // A copy-left license can *likely* consume another copy-left license.
                // You're not in the supported zone, so minor inconsistencies here are up to tthe end-user to verify.
                return;
            }
            else if ((license.LicenseType == LicenseManagement.LicenseType.Commercial || license.LicenseType == LicenseManagement.LicenseType.Other) &&
                (this.LicenseType == LicenseManagement.LicenseType.Commercial || this.LicenseType == LicenseManagement.LicenseType.Other))
            {
                // A commercial / other license is compatible with a commerical / other license -- with the end-user performing final validaiton.
                return;
            }

            // Note that this does not catch *all* incompatible cases, especially when delving into the realm of commercial or copy=left.
            throw new InvalidNapackException("The package " + napackName + "." + version + " has an incompatible license with the provided license.");
        }

        public bool NeedsMajorUpversioning(License license)
        {
            // Any license type or text changes require upversioning.
            return (this.LicenseType != license.LicenseType || !this.LicenseText.Equals(license.LicenseText, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}