using System;

namespace Napack.Common
{
    public class ConcurrentOperationException : Exception
    {
        public ConcurrentOperationException()
            : this("The operation was attempted to be performed exactly when another operation was in-progress.")
        {
        }

        public ConcurrentOperationException(string message)
            : base($"ConcurrentOperationException: {message}")
        {
        }
    }
}