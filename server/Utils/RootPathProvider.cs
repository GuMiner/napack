using Nancy;
using System;

namespace Napack.Server
{
    /// <summary>
    /// Modifies the root path to get it from the current working directory.
    /// </summary>
    internal class RootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return Environment.CurrentDirectory;
        }
    }
}