using System;

namespace Napack.Common
{
    public class InvalidUserIdException : Exception
    {
        public InvalidUserIdException()
            : base("The specified user email is an invalid email address.")
        {
        }
    }
}