using System;

namespace Napack.Common
{
    public class InvalidNapackVersionException : Exception
    {
        public InvalidNapackVersionException()
            : base("The specified napack version string is invalid. A valid version string has is of the format name.major.minor.patch")
        {
        }
    }
}