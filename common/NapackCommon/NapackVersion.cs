using System.Collections.Generic;

namespace Napack.Common
{
    public class NapackVersion
    {
        /// <summary>
        /// Required for serialization.
        /// </summary>
        public NapackVersion()
        {
        }

        public NapackVersion(int major, int minor, int patch, List<string> authors, Dictionary<string, string> files, License license)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.Authors = authors;
            this.Files = files;
            this.License = license;
        }

        public int Major { get; set; }
        
        public int Minor { get; set; }

        public int Patch { get; set; }

        public List<string> Authors { get; set; }
        
        /// <summary>
        /// The files associated with the package.
        /// </summary>
        /// <remarks>
        /// The keys are the path of the file within the napack, the values are the files themselves
        /// </remarks>
        public Dictionary<string, string> Files { get; set; }

        /// <summary>
        /// The license of the package.
        /// </summary>
        public License License { get; set; }
    }
}