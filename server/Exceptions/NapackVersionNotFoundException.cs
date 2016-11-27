using System;

namespace Napack.Server
{
    public class NapackVersionNotFoundException : Exception
    {
        public NapackVersionNotFoundException(int major, int? minor = null, int? patch = null) 
            : base("The specified napack version was not found. Major: " + major + ". Minor: " + minor + ". Patch: " + patch)
        {
        }
    }
}
