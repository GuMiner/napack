using System;
using System.Collections.Generic;

namespace Napack.Server
{
    public class NapackVersion
    {
        /// <summary>
        /// The minor version of this package.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// The patch version of this package.
        /// </summary>
        public int Patch { get; set; }

        /// <summary>
        /// The authors of this version of the package.
        /// </summary>
        public List<string> Authors { get; set; }
        
        /// <summary>
        /// The files associated with the package.
        /// </summary>
        /// <remarks>
        /// The keys are the path of the file, the values are IDs of package file references.
        /// </remarks>
        public Dictionary<string, Guid> Files { get; set; }

        /// <summary>
        /// Returns a JSON-serializable summary of this napack version.
        /// </summary>
        public object AsSummaryJson()
        {
            return new
            {
                this.Minor,
                this.Patch,
                this.Authors,
                Files = this.Files.Keys
            };
        }
    }
}