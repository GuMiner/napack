namespace Napack.Common
{
    /// <summary>
    /// Defines a single major version of a Napack.
    /// </summary>
    public class NapackMajorVersion
    {
        /// <summary>
        /// Required for serialization.
        /// </summary>
        public NapackMajorVersion()
        {
        }

        public NapackMajorVersion(string name, int major)
        {
            this.Name = name;
            this.Major = major;
        }

        /// <summary>
        /// The name of this napack.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The major version of this package.
        /// </summary>
        public int Major { get; set; }
    }
}