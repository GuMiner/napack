using System;

namespace Napack.Common
{
    public class InvalidNamespaceException : Exception
    {
        public InvalidNamespaceException(string file)
            : base("The class namespace is not in the namespace of the napack name. File: " + file)
        {
        }
    }
}