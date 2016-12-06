using System;

namespace Napack.Common
{
    public class ExcessiveNapackException : Exception
    {
        public ExcessiveNapackException()
            : base("The provided napack data exceeds the maximum size of a Napack.")
        {
        }
    }
}