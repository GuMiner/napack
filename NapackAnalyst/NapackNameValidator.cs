using System;
using Napack.Common;

namespace Napack.Analyst
{
    /// <summary>
    /// Deals with Napack validation.
    /// </summary>
    public static class NapackNameValidator
    {
        /// <summary>
        /// Initializes the decency standards and naming limits.
        /// </summary>
        public static void Initialize()
        {
            // TODO figure out some way to store the file (zip, bidirectional hash, rot13, etc) so as to not be publishing explicit words along with the source code...
        }

        /// <summary>
        /// Validates the provided Napack name meets the setup decency standards and naming limits.
        /// </summary>
        /// <param name="napackName">The name of the napack.</param>
        /// <exception cref="InvalidNapackNameException">If the napack fails naming validation</exception>
        public static void Validate(string napackName)
        {
            throw new NotImplementedException();
        }
    }
}
