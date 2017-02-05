using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
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
        private static readonly Dictionary<string, List<NapackVersionIdentifier>> authorPackageStore;

        /// <summary>
        /// The listing of user id => user.
        /// </summary>
        private static readonly Dictionary<string, UserIdentifier> users;

        /// <summary>
        /// The listing of user id => authorized napack name.
        /// </summary>
        private static readonly Dictionary<string, HashSet<string>> authorizedPackages;

        /// <summary>
        /// The listing of package major version => napacks on this server consuming said package.
        /// </summary>
        private static readonly Dictionary<string, List<NapackVersionIdentifier>> consumingPackages;

        /// <summary>
        /// The listing of package name => package metadata
        /// </summary>
        private static readonly Dictionary<string, NapackMetadata> packageMetadataStore;

        /// <summary>
        /// The listing of package name => package statistics.
        /// </summary>
        private static readonly Dictionary<string, NapackStats> statsStore;

        /// <summary>
        /// The listing of package identifier => package
        /// </summary>
        private static readonly Dictionary<string, NapackVersion> packageStore;

        /// <summary>
        /// The listing of package identifier => package specification
        /// </summary>
        private static readonly Dictionary<string, NapackSpec> specStore;

        /// <summary>
        /// The listing of search indicies for package searching, mapping package name -> search index.
        /// </summary>
        private static readonly Dictionary<string, NapackSearchIndex> searchIndices;

        static InMemoryNapackStorageManager()
        {
            authorPackageStore = new Dictionary<string, List<NapackVersionIdentifier>>(StringComparer.InvariantCultureIgnoreCase); // Author names are case insensitive
            users = new Dictionary<string, UserIdentifier>(StringComparer.InvariantCultureIgnoreCase);
            authorizedPackages = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCulture); // User names are case sensitive.
            consumingPackages = new Dictionary<string, List<NapackVersionIdentifier>>(StringComparer.InvariantCulture);
            packageMetadataStore = new Dictionary<string, NapackMetadata>(StringComparer.InvariantCulture);
            statsStore = new Dictionary<string, NapackStats>(StringComparer.InvariantCulture);
            packageStore = new Dictionary<string, NapackVersion>(StringComparer.InvariantCulture);
            specStore = new Dictionary<string, NapackSpec>(StringComparer.InvariantCulture);
            searchIndices = new Dictionary<string, NapackSearchIndex>(StringComparer.InvariantCulture);
        }

        public InMemoryNapackStorageManager()
        {
        }

        public bool ContainsNapack(string packageName)
        {
            return packageMetadataStore.ContainsKey(packageName);
        }

        public bool PerformsAutomatedBackups => false;

        public void RunDbBackup(object sender, ElapsedEventArgs e)
        {
            // No use implementing this
        }

        public List<NapackSearchIndex> FindPackages(string searchPhrase, int skip, int top)
        {
            // Implement the in-memory search by exact token-matching (all results have a relevance of 1, which isn't filled in).
            // TODO don't ignore tags.
            IEnumerable<NapackSearchIndex> results = searchIndices.Values;
            foreach (string phrase in searchPhrase.Split(' '))
            {
                results = results.Where(result => result.Description.Contains(phrase));
            }

            return results.Skip(skip).Take(top).ToList();
        }
        
        public void AddUser(UserIdentifier user)
        {
            if (users.ContainsKey(user.Email))
            {
                throw new ExistingUserException(user.Email);
            }

            users.Add(user.Email, user);
        }

        public UserIdentifier GetUser(string userId)
        {
            return users[userId];
        }
        
        public void UpdateUser(UserIdentifier user)
        {
            users[user.Email] = user;
        }

        public void RemoveUser(UserIdentifier user)
        {
            users.Remove(user.Email);
            authorizedPackages.Remove(user.Email);
        }

        public IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName)
        {
            return authorPackageStore[authorName];
        }

        public IEnumerable<string> GetAuthorizedPackages(string userId)
        {
            return authorizedPackages[userId];
        }

        public IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion)
        {
            return consumingPackages[packageMajorVersion.ToString()];
        }
        
        public NapackStats GetPackageStatistics(string packageName)
        {
            return statsStore[packageName];
        }

        public NapackSpec GetPackageSpecification(NapackVersionIdentifier packageVersion)
        {
            return specStore[packageVersion.GetFullName()];
        }

        public void IncrementPackageDownload(string packageName)
        {
            statsStore[packageName].Downloads++;
            searchIndices[packageName].Downloads++;
        }

        public NapackMetadata GetPackageMetadata(string packageName)
        {
            return packageMetadataStore[packageName];
        }

        public NapackVersion GetPackageVersion(NapackVersionIdentifier packageVersion)
        {
            return packageStore[packageVersion.GetFullName()];
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
            
            foreach (string userId in newNapack.metadata.AuthorizedUserIds)
            {
                AddUserAuthorization(userId, napackName);
            }

            foreach (NapackMajorVersion consumedPackage in newNapack.NewNapackVersion.Dependencies)
            {
                AddConsumingPackage(consumedPackage, version);
            }

            // Add the new napack to all the various stores.
            NapackStats stats = new NapackStats();
            stats.AddVersion(newNapack.NewNapackVersion);

            searchIndices.Add(version.NapackName, NapackSearchIndex.CreateFromMetadataAndStats(metadata, stats));
            
            statsStore.Add(version.NapackName, stats);
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

            UpdatePackageMetadataStore(package, nextVersion, upversionType, newNapackVersion);
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

            statsStore[nextVersion.NapackName].AddVersion(newNapackVersion);
            searchIndices[nextVersion.NapackName].LastUpdateTime = statsStore[nextVersion.NapackName].LastUpdateTime;
            searchIndices[nextVersion.NapackName].LastUsedLicense = newNapackVersion.License;

            packageMetadataStore[nextVersion.NapackName] = package;
        }

        private void AddConsumingPackage(NapackMajorVersion consumedPackage, NapackVersionIdentifier version)
        {
            if (!consumingPackages.ContainsKey(consumedPackage.ToString()))
            {
                consumingPackages.Add(consumedPackage.ToString(), new List<NapackVersionIdentifier>());
            }

            consumingPackages[consumedPackage.ToString()].Add(version);
        }

        private void AddUserAuthorization(string userId, string napackName)
        {
            if (!authorizedPackages.ContainsKey(userId))
            {
                authorizedPackages.Add(userId, new HashSet<string>());
            }
            
            authorizedPackages[userId].Add(napackName);
        }

        private void AddAuthorConsumption(string author, NapackVersionIdentifier version)
        {
            if (!authorPackageStore.ContainsKey(author))
            {
                authorPackageStore.Add(author, new List<NapackVersionIdentifier>());
            }

            authorPackageStore[author].Add(version);
        }

        public void UpdatePackageMetadata(NapackMetadata metadata)
        {
            packageMetadataStore[metadata.Name] = metadata;
        }

        public void UpdatePackageVersion(NapackVersionIdentifier packageVersion, NapackVersion updatedVersion)
        {
            if (packageStore.ContainsKey(packageVersion.GetFullName()))
            {
                packageStore[packageVersion.GetFullName()] = updatedVersion;
            }
        }

        public void RemovePackageVersion(NapackVersionIdentifier packageVersion)
        {
            packageStore.Remove(packageVersion.GetFullName());
        }

        public void RemovePackageSpecification(NapackVersionIdentifier packageVersion)
        {
            specStore.Remove(packageVersion.GetFullName());
        }

        public void RemovePackageStatistics(string packageName)
        {
            statsStore.Remove(packageName);
        }

        public void RemoveAuthoredPackages(string authorName, string packageName)
        {
            if (authorizedPackages.ContainsKey(authorName))
            {
                authorizedPackages[authorName].Remove(packageName);
            }
        }
    }
}