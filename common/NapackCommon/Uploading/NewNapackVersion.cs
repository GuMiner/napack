using System.Collections.Generic;

namespace Napack.Common
{
    /// <summary>
    /// Lists the necessary fields for adding a new napack version to an existing Napack.
    /// </summary>
    /// <remarks>
    /// The URI defines the napack being updated.
    /// The type of update (major/minor/patch) is automatically computed from the change in dependencies and public API surface on the files.
    /// If the files have no publically-facing changes and dependencies are not added => patch change.
    /// If the files have no publically-breaking changes (but do have publically-facing changes) or dependencies are added => minor change.
    /// If the files have publically-breaking API changes or dependencies are updated => major change.
    /// </remarks>
    public class NewNapackVersion
    {
        /// <summary>
        /// Authors
        /// </summary>
        public List<string> Authors { get; set; }
        
        /// <summary>
        /// The files associated with the package.
        /// </summary>
        /// <remarks>
        /// The keys are the path of the file within the napack, the values are the files themselves
        /// </remarks>
        public Dictionary<string, NapackFile> Files { get; set; }

        /// <summary>
        /// The license of the package.
        /// </summary>
        public License License { get; set; }

        /// <summary>
        /// The dependent napacks for this package.
        /// </summary>
        public List<NapackMajorVersion> Dependencies { get; set; }
    }
}