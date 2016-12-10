using System.Collections.Generic;
using Napack.Common;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines the publically-facing API of a Napack
    /// </summary>
    public class NapackSpec
    {
        public NapackSpec()
        {
            this.UnknownFiles = new List<NapackFile>();
            this.Classes = new List<ClassSpec>();
        }

        /// <summary>
        /// Unknown files that are not compilable by the Roslyn API.
        /// </summary>
        /// <remarks>
        /// This list is guaranteed to not have anything with the <see cref="NapackFile.ContentType"/> type, 
        ///  as all those files must be parseable by the API.
        /// </remarks>
        public List<NapackFile> UnknownFiles { get; set; }

        /// <summary>
        /// The classes of this Napack *that are publically visible*
        /// </summary>
        /// <remarks>
        /// All fields listed within other *Spec files only list publically-visible types.
        /// </remarks>
        public List<ClassSpec> Classes { get; set; }
    }
}
