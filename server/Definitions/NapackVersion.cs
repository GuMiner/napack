using System.Collections.Generic;

namespace Napack.Server
{
    public class NapackVersion
    {
        /// <summary>
        /// The authors of this version of the package.
        /// </summary>
        public List<string> Authors { get; set; }
        
        /// <summary>
        /// The files associated with the package.
        /// </summary>
        /// <remarks>
        /// The keys are the path of the file, the values are the files themselves
        /// </remarks>
        public Dictionary<string, string> Files { get; set; }

        /// <summary>
        /// Returns a JSON-serializable summary of this napack version.
        /// </summary>
        public object AsSummaryJson()
        {
            return new
            {
                this.Authors,
                Files = this.Files.Keys,
            };
        }
    }
}