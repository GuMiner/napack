using System.Collections.Generic;

namespace Napack.Analyst
{
    /// <summary>
    /// Defines the name validation configuration.
    /// </summary>
    public class NameValidationConfig
    {
        public string NameValidationRegex { get; set; }

        public int NameLengthMinLimit { get; set; }

        public int NameLengthMaxLimit { get; set; }

        /// <summary>
        /// Gets the basic list of prohibited subtrings.
        /// </summary>
        public List<string> ProhibitedSubstrings { get; set; }

        /// <summary>
        /// Gets the character conversion list to formulate a new string.
        /// </summary>
        /// <remarks>
        /// The character conversion dictionary is intended to list common character lookalikes,
        /// such as 1 and l. These lookalikes are used to perform additional validation that 
        /// no prohibited substrings are included the Napack name.
        /// </remarks>
        public Dictionary<char, char> CharacterConversions { get; set; }
    }
}
