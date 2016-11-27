using System;

namespace NapackClient
{
    public class InvalidNapackVersionException : Exception
    {
        public InvalidNapackVersionException(string napackVersion)
            : base("The specified napack version string is invalid: " + napackVersion)
        {
        }
    }
}