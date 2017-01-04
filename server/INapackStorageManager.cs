using System.Collections.Generic;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Manages Napack storage.
    /// </summary>
    public interface INapackStorageManager
    {
        /// <summary>
        /// Returns true if the napack exists, false otherwise.
        /// </summary>
        bool ContainsNapack(string packageName);

        /// <summary>
        /// Gets the metadata for the specified package name.
        /// </summary>
        NapackMetadata GetPackageMetadata(string packageName);

        /// <summary>
        /// Gets the specified package version.
        /// </summary>
        NapackVersion GetPackageVersion(NapackVersionIdentifier packageVersion);

        /// <summary>
        /// Gets the Napack-determined publically-facing specification of the specified package version.
        /// </summary>
        NapackSpec GetPackageSpecification(NapackVersionIdentifier packageVersion);

        /// <summary>
        /// Gets all packages consuming the specified major version of a package.
        /// </summary>
        /// <remarks>
        /// The reverse of this operation can be done by reading the dependencies specified in the metadata.
        /// </remarks>
        IEnumerable<NapackVersionIdentifier> GetPackageConsumers(NapackMajorVersion packageMajorVersion);
        
        /// <summary>
        /// Adds a user for future security validation.
        /// </summary>
        void AddUser(UserIdentifier user);

        /// <summary>
        /// Updates a user.
        /// </summary>
        void UpdateUser(UserIdentifier user);

        /// <summary>
        /// Gets the statistics for a Napack package.
        /// </summary>
        NapackStats GetPackageStatistics(string packageName);

        /// <summary>
        /// Adds a download to the download count for this Napack package
        /// </summary>
        void IncrementPackageDownload(string packageName);

        /// <summary>
        /// Gets the statistics, hash, and other information stored for the given user.
        /// </summary>
        UserIdentifier GetUser(string userId);

        /// <summary>
        /// Gets all package names the user is authorized to edit.
        /// </summary>
        IEnumerable<string> GetAuthorizedPackages(string userId);

        /// <summary>
        /// Gets all package version for which the given author is listed in the authors list.
        /// </summary>
        IEnumerable<NapackVersionIdentifier> GetAuthoredPackages(string authorName);

        /// <summary>
        /// Performs a search through the package name, description, more information, and tags
        ///  to find relevant packages.
        /// </summary>
        /// <param name="searchPhrase">The search phrase to use.</param>
        /// <param name="skip">The number of results to skip.</param>
        /// <param name="top">The maximum number of results to return.</param>
        /// <returns>Relevant package -> match percentage</returns>
        List<NapackSearchIndex> FindPackages(string searchPhrase, int skip, int top);

        /// <summary>
        /// Updates the metadata of a package with ... updated metadata.
        /// </summary>
        void UpdatePackageMetadata(NapackMetadata metadata);

        /// <summary>
        /// Removes a user. Assumes the user has already been removed from all packages the user is authorized to deal with.
        /// </summary>
        void RemoveUser(UserIdentifier user);

        /// <summary>
        /// Creates and saves a new napack.
        /// </summary>
        /// <param name="napackName">The name of the napack</param>
        /// <param name="newNapack">The new napack to create/save</param>
        /// <param name="napackSpec">The napack specification</param>
        void SaveNewNapack(string napackName, NewNapack newNapack, NapackSpec napackSpec);

        /// <summary>
        /// Saves a new napack version onto an existing napack.
        /// </summary>
        /// <param name="package">The package metadata.</param>
        /// <param name="currentVersion">The current version of the napack.</param>
        /// <param name="upversionType">The type of upversioning to perform.</param>
        /// <param name="newNapackVersion">The new napack version.</param>
        /// <param name="newVersionSpec">The napack specification.</param>
        void SaveNewNapackVersion(NapackMetadata package, NapackVersionIdentifier currentVersion, NapackAnalyst.UpversionType upversionType, NewNapackVersion newNapackVersion, NapackSpec newVersionSpec);
    }
}