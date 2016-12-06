namespace Napack.Common
{
    /// <summary>
    /// Defines a file within a Napack that can be linked to a project.
    /// </summary>
    public class NapackFile
    {
        public const string ContentType = "Content";

        /// <summary>
        /// The type of item within MSBUILD.
        /// </summary>
        /// <remarks>
        /// Most items will either be 'None', 'EmbeddedResource', or 'Content'. 
        /// </remarks>
        public string MsbuildType { get; set; }

        /// <summary>
        /// The file contents of this file.
        /// </summary>
        public string Contents { get; set; }
    }
}
