using System;
using System.Linq;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Defines the result of a Napack Search.
    /// </summary>
    public class NapackSearchIndex
    {
        /// <summary>
        /// Replaced by the search algorithm when retrieved.
        /// </summary>
        public float Relevance { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public long Downloads { get; set; }

        public long Views { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public License LastUsedLicense { get; set; }

        public static NapackSearchIndex CreateFromMetadataAndStats(NapackMetadata metadata, NapackStats stats)
        {
            return new NapackSearchIndex()
            {
                Name = metadata.Name,
                Description = metadata.Description,
                LastUsedLicense = metadata.Versions[metadata.Versions.Keys.Max()].License,
                Downloads = stats.Downloads,
                Views = stats.Views,
                LastUpdateTime = stats.LastUpdateTime,
            };
        }

        internal dynamic ToAnonymousType()
        {
            return new
            {
                Relevance = this.Relevance,
                Name = this.Name,
                Description = this.Description,
                Downloads = this.Downloads,
                Views = this.Views,
                LastUpdateTime = this.LastUpdateTime.ToString("U"),
                LicenseType = LicenseManagement.GetLicenseName(this.LastUsedLicense.LicenseType)
            };
        }
    }
}