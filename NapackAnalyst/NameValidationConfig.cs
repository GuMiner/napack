using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Napack.Common;

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
        /// Converts the prohibited substrings from base 64 format to computer-usable format.
        /// </summary>
        public void ReverseEncodeProhibitedSubstrings()
        {
            this.ProhibitedSubstrings = this.ProhibitedSubstrings
                .Select(substring => Encoding.UTF8.GetString(Convert.FromBase64String(substring)))
                .ToList();
        }

        /// <summary>
        /// Validates the provided Napack name meets the setup decency standards and naming limits.
        /// </summary>
        /// <param name="napackName">The name of the napack.</param>
        /// <exception cref="InvalidNapackNameException">If the napack fails naming validation</exception>
        public void Validate(string napackName)
        {
            if (napackName.Length < this.NameLengthMinLimit)
            {
                throw new InvalidNapackNameException("The name is too short!");
            }
            else if (napackName.Length > this.NameLengthMaxLimit)
            {
                throw new InvalidNapackNameException("The name is too long!");
            }
            else if (!Regex.Match(napackName, NameValidationRegex, RegexOptions.Compiled).Success)
            {
                throw new InvalidNapackNameException("The name does not match the name rule regex!");
            }
            else if (this.ProhibitedSubstrings.Any(substring => napackName.IndexOf(substring, StringComparison.InvariantCultureIgnoreCase) != -1))
            {
                throw new InvalidNapackNameException("A prohibited substring was found within the napack name.");
            }
        }
    }
}
