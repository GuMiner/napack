using System;

namespace Napack.Common
{
    public class InvalidNapackFileException : Exception
    {
        public InvalidNapackFileException(string file, string details)
            : base("A code file '" + file + "'could not be analyzed for API compatibility. Details: " + details)
        {
        }
    }
}