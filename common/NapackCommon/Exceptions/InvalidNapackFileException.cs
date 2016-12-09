using System;

namespace Napack.Common
{
    public class InvalidNapackFileException : Exception
    {
        public InvalidNapackFileException(string details)
            : base("A code file could not be analyzed for API compatibility. Details: " + details)
        {
        }
    }
}