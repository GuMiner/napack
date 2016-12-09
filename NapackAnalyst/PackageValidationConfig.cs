using System;
using System.Collections.Generic;
using System.Linq;
using Napack.Common;

namespace Napack.Analyst
{
    /// <summary>
    /// Defines the name validation configuration.
    /// </summary>
    public class PackageValidationConfig
    {
        /// <summary>
        /// Gets a dictionary where each entry is a prohibited extension category and the prohibited extensions under that category.
        /// </summary>
        public Dictionary<string, List<string>> ProhibitedExtensions { get; set; }
        
        /// <summary>
        /// Validates the provided extension is not in the prohibition list.
        /// </summary>
        /// <exception cref="InvalidNapackFileExtensionException">If the extension is on the prohibition list.</exception>
        public void ValidateExtension(string filename, string extension)
        {
            foreach (KeyValuePair<string, List<string>> prohibitionList in this.ProhibitedExtensions)
            {
                if (prohibitionList.Value.Any(prohibition => prohibition.Equals(extension, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new InvalidNapackFileExtensionException(filename, extension);
                }
            }
        }
    }
}
