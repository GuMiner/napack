﻿using System.Collections.Generic;
using System.Linq;
using Napack.Common;

namespace Napack.Server
{
    public class NapackMajorVersion
    {
        /// <summary>
        /// Returns true if this package version has been recalled.
        /// </summary>
        /// <remarks>
        /// Recalling a package is an admin-only action that is *not* intended to be hit in normal practise.
        /// </remarks>
        public bool Recalled { get; set; }

        /// <summary>
        /// The major version of this package.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// The individual versions all under this major version.
        /// </summary>
        public List<NapackVersion> Versions { get; set; }

        /// <summary>
        /// The license of the package.
        /// </summary>
        public License License { get; set; }

        /// <summary>
        /// Gets the server-side version of a <see cref="NapackVersion"/>
        /// </summary>
        public NapackVersion GetVersion(int minorVersion, int patchVersion)
        {
            NapackVersion foundVersion = this.Versions.FirstOrDefault(version => version.Minor == minorVersion && version.Patch == patchVersion);
            if (foundVersion == null)
            {
                throw new NapackVersionNotFoundException(this.Major, minorVersion, patchVersion);
            }

            return foundVersion;
        }

        /// <summary>
        /// Gets the client-side version of a <see cref="Common.NapackVersion"/>
        /// </summary>
        public Common.NapackVersion GetDownloadableVersion(int minorVersion, int patchVersion)
        {
            NapackVersion foundVersion = this.Versions.FirstOrDefault(version => version.Minor == minorVersion && version.Patch == patchVersion);
            if (foundVersion == null)
            {
                throw new NapackVersionNotFoundException(this.Major, minorVersion, patchVersion);
            }

            return new Common.NapackVersion(this.Major, foundVersion.Minor, foundVersion.Patch, foundVersion.Authors, foundVersion.Files, this.License, foundVersion.Dependencies);
        }

        /// <summary>
        /// Lists the summary of this major version as a dynamic object.
        /// </summary>
        /// <param name="specifiedMinorVersion">If null, lists all minor versions. Otherwise, all minor versions matching the specified ID are listed.</param>
        public object AsSummaryJson(int? specifiedMinorVersion)
        {
            return new
            {
                Recalled = this.Recalled,
                Major = this.Major,
                Versions = this.Versions
                    .Where(version => specifiedMinorVersion == null || version.Minor == specifiedMinorVersion)
                    .Select(version => version.Minor + "." + version.Patch),
                License = this.License.AsSummaryJson(),
            };
        }
    }
}