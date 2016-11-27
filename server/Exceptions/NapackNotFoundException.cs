using System;

namespace Napack.Server
{
    public class NapackNotFoundException : Exception
    {
        public NapackNotFoundException(string napackName) 
            : base("The " + napackName + " napack package was not found.")
        {
        }
    }
}
