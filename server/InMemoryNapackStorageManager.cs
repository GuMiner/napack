using System;
using System.Collections.Generic;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Manages storage of Napacks and their files using local memory. Useful for diagnosis and local testing.
    /// Not intended for actual operation -- this isn't thread safe, for starters...
    /// </summary>
    public class InMemoryNapackStorageManager : INapackStorageManager
    {
        /// <summary>
        /// The listing of author => authored package identifiers
        /// </summary>
        private readonly Dictionary<string, List<NapackVersionIdentifier>> authorPackageStore;

        /// <summary>
        /// The listing of user hash => authorized packages.
        /// </summary>
        private readonly Dictionary<string, List<string>> authorizedPackages;

        /// <summary>
        /// The listing of package major version => napacks on this server consuming said package.
        /// </summary>
        private readonly Dictionary<string, List<NapackVersionIdentifier>> consumingPackages;

        /// <summary>
        /// The listing of package name => package metadata
        /// </summary>
        private readonly Dictionary<string, NapackMetadata> packageMetadataStore;

        /// <summary>
        /// The listing of package identifier => package
        /// </summary>
        private readonly Dictionary<string, NapackVersion> packageStore;
        

        public InMemoryNapackStorageManager()
        {
            this.authorPackageStore = new Dictionary<string, List<NapackVersionIdentifier>>(StringComparer.InvariantCultureIgnoreCase);
            this.authorizedPackages = new Dictionary<string, List<string>>(StringComparer.InvariantCulture);
            this.consumingPackages = new Dictionary<string, List<NapackVersionIdentifier>>(StringComparer.InvariantCultureIgnoreCase);
            this.packageMetadataStore = new Dictionary<string, NapackMetadata>(StringComparer.InvariantCultureIgnoreCase);
            this.packageStore = new Dictionary<string, NapackVersion>(StringComparer.InvariantCultureIgnoreCase);
        }

        public bool ContainsNapack(string packageName)
        {
            return this.packageStore.ContainsKey(packageName);
        }

        public IDictionary<string, float> FindPackages(string searchPhrase, int skip, int top)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName)
        {
            return this.authorPackageStore[authorName];
        }

        public IEnumerable<string> GetAuthorizedPackages(string userHash)
        {
            return this.authorizedPackages[userHash];
        }

        public IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
        {
            return this.consumingPackages[packageMajorVersion.ToString()];
        }

        public NapackMetadata GetPackageMetadata(string packageName)
        {
            return this.packageMetadataStore[packageName];
        }

        public NapackVersion GetPackageVersion(NapackVersionIdentifier packageVersion)
        {
            return this.packageStore[packageVersion.GetFullName()];
        }
    }
}