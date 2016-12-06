using System.Collections.Generic;

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
        NapackVersion GetPackageVersion(Common.NapackVersionIdentifier packageVersion);

        /// <summary>
        /// Gets all packages consuming the specified major version of a package.
        /// </summary>
        /// <remarks>
        /// The reverse of this operation can be done by reading the dependencies specified in the metadata.
        /// </remarks>
        IEnumerable<Common.NapackVersionIdentifier> GetPackageConsumers(Common.NapackMajorVersion packageMajorVersion);

        /// <summary>
        /// Gets all package names the user is authorized to edit.
        /// </summary>
        IEnumerable<string> GetAuthorizedPackages(string userHash);

        /// <summary>
        /// Gets all package version for which the given author is listed in the authors list.
        /// </summary>
        IEnumerable<Common.NapackVersionIdentifier> GetAuthoredPackages(string authorName);

        /// <summary>
        /// Performs a search through the package name, description, more information, and tags
        ///  to find relevant packages.
        /// </summary>
        /// <param name="searchPhrase">The search phrase to use.</param>
        /// <param name="skip">The number of results to skip.</param>
        /// <param name="top">The maximum number of results to return.</param>
        /// <returns>Relevant package -> match percentage</returns>
        IDictionary<string, float> FindPackages(string searchPhrase, int skip, int top);
    }
}