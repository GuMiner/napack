using System;

namespace Napack.Common
{
    public class InvalidNapackNameException : Exception
    {
        public InvalidNapackNameException(string failingPart)
            : base("The specified napack name failed name validation: " + failingPart)
        {
        }
    }
}