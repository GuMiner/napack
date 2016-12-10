using System;

namespace Napack.Client.Common
{
    public class NapackFrameworkServerUnavailable : Exception
    {
        public NapackFrameworkServerUnavailable(string errorMessage)
            : base("The Napack Framework Server is unavailable: " + errorMessage)
        {
        }
    }
}