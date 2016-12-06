using System;

namespace Napack.Common
{
    public class NapackRecalledException : Exception
    {
        public NapackRecalledException(string napackName, int majorVersion)
            : base("This napack major version was recalled and can no longer be downloaded. Napack: " + napackName + "." + majorVersion)
        {
        }
    }
}