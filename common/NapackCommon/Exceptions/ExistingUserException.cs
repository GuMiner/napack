using System;

namespace Napack.Common
{
    public class ExistingUserException : Exception
    {
        public ExistingUserException(string userId)
            : base($"The user '{userId}' already exists in the Napack Framework Server.")
        {
        }
    }
}