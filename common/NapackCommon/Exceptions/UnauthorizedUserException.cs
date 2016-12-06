using System;

namespace Napack.Common
{
    public class UnauthorizedUserException : Exception
    {
        public UnauthorizedUserException()
            : base("The specified user does not have permission to perform this napack operation.")
        {
        }
    }
}