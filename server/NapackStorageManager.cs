using System;

namespace Napack.Server
{
    /// <summary>
    /// Manages storage of Napacks and their files.
    /// </summary>
    public class NapackStorageManager : INapackStorageManager
    {
        /// <summary>
        /// Gets the specified napack package from the database.
        /// </summary>
        public NapackPackage GetPackage(string packageName)
        {
            // TODO setup unit testing of the modules and fill in the interface.
            throw new NapackNotFoundException(packageName);
        }
    }
}