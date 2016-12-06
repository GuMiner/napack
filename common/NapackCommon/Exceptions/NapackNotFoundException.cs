using System;

namespace Napack.Common
{
    public class NapackNotFoundException : Exception
    {
        public NapackNotFoundException(string napackName) 
            : base("The " + napackName + " napack package was not found.")
        {
        }
    }
}
