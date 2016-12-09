using System;

namespace Napack.Common
{
    public class UnsupportedNapackFileException : Exception
    {
        public UnsupportedNapackFileException(string file)
            : base("A napack file uses C# functionality that the Napack system does not support. File: " + file)
        {
        }
    }
}