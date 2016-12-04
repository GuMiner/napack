using System;
using System.Collections.Generic;

namespace Napack.Server
{
    /// <summary>
    /// Manages storage of Napacks and their files using local memory. Useful for diagnosis and local testing.
    /// </summary>
    public class InMemoryNapackStorageManager : INapackStorageManager
    {
        private readonly Dictionary<string, NapackPackage> packages;

        public InMemoryNapackStorageManager()
        {
            this.packages = new Dictionary<string, NapackPackage>(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the specified napack package from the database.
        /// </summary>
        public NapackPackage GetPackage(string packageName)
        {
            if (this.packages.ContainsKey(packageName))
            {
                return this.packages[packageName];
            }

            throw new NapackNotFoundException(packageName);
        }

        public void UpdatePackage(NapackPackage package)
        {
            this.packages[package.Name] = package;
        }

        public List<NapackMajorVersion> GetFlattenedPackageVersionDependencies(string packageName)
        {
            throw new NotImplementedException();
        }

        public List<NapackMajorVersion> GetPackageVersionDependencies(string packageName)
        {
            throw new NotImplementedException();
        }
    }
}