using System.Collections.Generic;
using System.Linq;

namespace Napack.Server
{
    public class NapackVersion
    {
        public int Minor { get; set; }

        public int Patch { get; set; }

        // TODO we definitely need uploader.

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
        /// The dependent napacks for this Napack version.
        /// </summary>
        public List<Common.NapackMajorVersion> Dependencies { get; set; }

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
                Files = this.Files.Keys,
                Dependencies = this.Dependencies.Select(dependency => dependency.Name + "." + dependency.Major)
            };
        }
    }
}