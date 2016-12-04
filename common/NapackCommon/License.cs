using System;

namespace Napack.Common
{
    public class License
    {
        /// <summary>
        /// The name of the license.
        /// </summary>
        public LicenseManagement.LicenseType LicenseName { get; set; }

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
                Name = this.LicenseName,
                this.IsSupportedLicense,
                this.IsCopyLeft,
                this.IsCommercial,
                this.IsCustomLicense
            };
        }
    }
}