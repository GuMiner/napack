using System;

namespace Napack.Common
{
    public class InvalidNapackFileExtensionException : Exception
    {
        public InvalidNapackFileExtensionException(string file, string extension)
            : base("A napack file has a prohibited extension '" + extension + "'. File: " + file)
        {
        }
    }
}