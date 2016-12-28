using System;
using System.IO;

namespace Napack.Client
{
    public static class PathUtilities
    {
        public static string GetRelativePath(string rootDirectory, string file)
        {
            Uri filePathUri = new Uri(file);
            if (!rootDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                rootDirectory += Path.DirectorySeparatorChar;
            }

            return Uri.UnescapeDataString(new Uri(rootDirectory).MakeRelativeUri(filePathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
