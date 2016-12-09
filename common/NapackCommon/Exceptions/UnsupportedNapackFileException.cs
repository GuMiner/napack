using System;

namespace Napack.Common
{
    public class UnsupportedNapackFileException : Exception
    {
        public UnsupportedNapackFileException(string file, string usage)
            : base("A napack file uses '" + usage + "'C# functionality that the Napack system does not support. File: " + file)
        {
        }
    }
}