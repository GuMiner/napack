using System.Collections.Generic;
using System.Linq;
using Napack.Common;

namespace Napack.Server
{
    public class NapackMajorVersionMetadata
    {
        /// <summary>
        /// Returns true if this package version has been recalled.
        /// </summary>
        /// <remarks>
        /// Recalling a package is an admin-only action that is *not* intended to be hit in normal practise.
        /// </remarks>
        public bool Recalled { get; set; }

        /// <summary>
        /// The individual versions all under this major version, stored in minor => patch format.
        /// </summary>
        public Dictionary<int, List<int>> Versions { get; set; }

        /// <summary>
        /// The license of the package.
        /// </summary>
        public License License { get; set; }

        /// <summary>
        /// Lists the summary of this major version as a dynamic object.
        /// </summary>
        public object AsSummaryJson()
        {
            return new
            {
                Recalled = this.Recalled,
                Versions = this.Versions.SelectMany(version => version.Value.Select(patchVersion => version.Key + "." + patchVersion)),
                License = this.License.AsSummaryJson(),
            };
        }
    }
}