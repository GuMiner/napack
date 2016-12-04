using System;

namespace Napack.Common
{
    public class InvalidNapackVersionException : Exception
    {
        public InvalidNapackVersionException(string napackVersion)
            : base("The specified napack version string is invalid: " + napackVersion)
        {
        }
    }
}