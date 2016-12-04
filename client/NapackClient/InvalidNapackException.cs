using System;

namespace Napack.Client
{
    internal class InvalidNapackException : Exception
    {
        public InvalidNapackException()
            : base("The specified napack is invalid!")
        {
        }

        public InvalidNapackException(string message)
            : base("The specified napack is invalid: " + message)
        {
        }
    }
}