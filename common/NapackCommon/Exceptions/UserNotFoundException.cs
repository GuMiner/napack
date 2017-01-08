using System;

namespace Napack.Common
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userId)
            : base($"The user '{userId}' does not exist.")
        {
        }
    }
}