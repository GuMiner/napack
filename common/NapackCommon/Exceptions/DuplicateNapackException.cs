using System;

namespace Napack.Common
{
    public class DuplicateNapackException : Exception
    {
        public DuplicateNapackException()
            : base("A napack with the specified name already exists.")
        {
        }
    }
}