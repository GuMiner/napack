using System;
using System.Collections.Generic;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Manages storage of Napacks and their files using local memory. Useful for diagnosis and local testing.
    /// **Not intended for actual operation -- this isn't thread safe, for starters...**
    /// </summary>
    public class InMemoryNapackStorageManager : INapackStorageManager
    {
        /// <summary>
        /// The listing of author => authored package identifiers
        /// </summary>
        private readonly Dictionary<string, List<NapackVersionIdentifier>> authorPackageStore;

        /// <summary>
        /// The listing of user hash => authorized napack name.
        /// </summary>
        private readonly Dictionary<string, HashSet<string>> authorizedPackages;

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

        /// <summary>
        /// The listing of package identifier => package specification
        /// </summary>
        private readonly Dictionary<string, NapackSpec> specStore;

        public InMemoryNapackStorageManager()
        {
            this.authorPackageStore = new Dictionary<string, List<NapackVersionIdentifier>>(StringComparer.InvariantCultureIgnoreCase); // Author names are case insensitive
            this.authorizedPackages = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCulture); // Hashes and package names are case-sensitive.
            this.consumingPackages = new Dictionary<string, List<NapackVersionIdentifier>>(StringComparer.InvariantCulture);
            this.packageMetadataStore = new Dictionary<string, NapackMetadata>(StringComparer.InvariantCulture);
            this.packageStore = new Dictionary<string, NapackVersion>(StringComparer.InvariantCulture);
            this.specStore = new Dictionary<string, NapackSpec>(StringComparer.InvariantCulture);
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

        public NapackSpec GetPackageSpecification(NapackVersionIdentifier packageVersion)
        {
            return this.specStore[packageVersion.GetFullName()];
        }

        /// <summary>
        /// Creates and saves a new napack.
        /// </summary>
        /// <param name="napackName">The name of the napack</param>
        /// <param name="newNapack">The new napack to create/save</param>
        /// <param name="napackSpec">The napack specification</param>
        public void SaveNewNapack(string napackName, NewNapack newNapack, NapackSpec napackSpec)
        {
            NapackVersionIdentifier version = new NapackVersionIdentifier(napackName, 1, 0, 0);
            NapackMetadata metadata = NapackMetadata.CreateFromNewNapack(napackName, newNapack);
            NapackVersion packageVersion = NapackVersion.CreateFromNewNapack(newNapack.NewNapackVersion);

            foreach (string author in newNapack.NewNapackVersion.Authors)
            {
                AddAuthorConsumption(author, version);
            }
            
            foreach (string userHash in newNapack.AuthorizedUserHashes)
            {
                AddUserAuthorization(userHash, napackName);
            }

            foreach (NapackMajorVersion consumedPackage in newNapack.NewNapackVersion.Dependencies)
            {
                AddConsumingPackage(consumedPackage, version);
            }

            // Add the new napack to all the various stores.
            packageMetadataStore.Add(version.NapackName, metadata);
            packageStore.Add(version.GetFullName(), packageVersion);
            specStore.Add(version.GetFullName(), napackSpec);
        }

        /// <summary>
        /// Saves a new napack version onto an existing napack.
        /// </summary>
        /// <param name="package">The package metadata.</param>
        /// <param name="currentVersion">The current version of the napack.</param>
        /// <param name="upversionType">The type of upversioning to perform.</param>
        /// <param name="newNapackVersion">The new napack version.</param>
        /// <param name="newVersionSpec">The napack specification.</param>
        public void SaveNewNapackVersion(NapackMetadata package, NapackVersionIdentifier currentVersion, NapackAnalyst.UpversionType upversionType, NewNapackVersion newNapackVersion, NapackSpec newVersionSpec)
        {
            NapackVersionIdentifier nextVersion = new NapackVersionIdentifier(currentVersion.NapackName, currentVersion.Major, currentVersion.Minor, currentVersion.Patch);
            NapackVersion packageVersion = NapackVersion.CreateFromNewNapack(newNapackVersion);

            foreach (string author in newNapackVersion.Authors)
            {
                AddAuthorConsumption(author, nextVersion);
            }

            // Changes in user authorization do not occur through napack version updates.

            foreach (NapackMajorVersion consumedPackage in newNapackVersion.Dependencies)
            {
                AddConsumingPackage(consumedPackage, nextVersion);
            }

            this.UpdatePackageMetadataStore(package, nextVersion, upversionType, newNapackVersion);
            packageStore.Add(nextVersion.GetFullName(), packageVersion);
            specStore.Add(nextVersion.GetFullName(), newVersionSpec);
        }

        private void UpdatePackageMetadataStore(NapackMetadata package, NapackVersionIdentifier nextVersion, NapackAnalyst.UpversionType upversionType, NewNapackVersion newNapackVersion)
        {
            if (upversionType == NapackAnalyst.UpversionType.Major)
            {
                NapackMajorVersionMetadata newMajorVersionMetadata = new NapackMajorVersionMetadata()
                {
                    Recalled = false,
                    Versions = new Dictionary<int, List<int>>
                    {
                        [0] = new List<int> { 0 }
                    },
                    License = newNapackVersion.License
                };

                package.Versions.Add(nextVersion.Major, newMajorVersionMetadata);
            }
            else if (upversionType == NapackAnalyst.UpversionType.Minor)
            {
                package.Versions[nextVersion.Major].Versions.Add(nextVersion.Minor, new List<int> { 0 });
            }
            else
            {
                package.Versions[nextVersion.Major].Versions[nextVersion.Minor].Add(nextVersion.Patch);
            }

            packageMetadataStore[nextVersion.NapackName] = package;
        }

        private void AddConsumingPackage(NapackMajorVersion consumedPackage, NapackVersionIdentifier version)
        {
            if (!this.consumingPackages.ContainsKey(consumedPackage.ToString()))
            {
                this.consumingPackages.Add(consumedPackage.ToString(), new List<NapackVersionIdentifier>());
            }

            this.consumingPackages[consumedPackage.ToString()].Add(version);
        }

        private void AddUserAuthorization(string userHash, string napackName)
        {
            if (!this.authorizedPackages.ContainsKey(userHash))
            {
                this.authorizedPackages.Add(userHash, new HashSet<string>());
            }
            
            this.authorizedPackages[userHash].Add(napackName);
        }

        private void AddAuthorConsumption(string author, NapackVersionIdentifier version)
        {
            if (!this.authorPackageStore.ContainsKey(author))
            {
                this.authorPackageStore.Add(author, new List<NapackVersionIdentifier>());
            }

            this.authorPackageStore[author].Add(version);
        }
    }
}