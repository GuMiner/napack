using System.Threading.Tasks;
using Napack.Common;

namespace Napack.Client.Common
{
    /// <summary>
    /// Defines the client-server communication mechanism the Napack Client uses.
    /// </summary>
    public interface INapackServerClient
    {
        /// <summary>
        /// Retrieves a <see cref="NapackVersion"/> from the Napack Framework Server.
        /// </summary>
        /// <param name="napackVersionDefinition">The napack version to retrieve.</param>
        /// <returns>The <see cref="NapackVersion"/> retrieved.</returns>
        /// <exception cref="NapackFrameworkServerUnavailable">If the Napack Framework Server is unavailable.</exception>
        /// <exception cref="NapackRecalledException">If the Napack was found, but is no longer available for download.</exception>
        /// <exception cref="NapackVersionNotFoundException">If the specified Napack version was not found.</exception>
        /// <exception cref="InvalidNapackException">If the retrieved Napack is invalid and cannot be deserialized.</exception>
        Task<NapackVersion> GetNapackVersionAsync(NapackVersionIdentifier napackVersionDefinition);

        /// <summary>
        /// Retrieves the most recent minor/patch version of a major version'd <see cref="NapackVersion"/> from the Napack Framework Server.
        /// </summary>
        /// <param name="partialNapackVersionDefinition">The partial napack version to retrieve.</param>
        /// <returns>The <see cref="NapackVersion"/> retrieved.</returns>
        /// <exception cref="NapackFrameworkServerUnavailable">If the Napack Framework Server is unavailable.</exception>
        /// <exception cref="NapackRecalledException">If the Napack was found, but is no longer available for download.</exception>
        /// <exception cref="NapackVersionNotFoundException">If the specified Napack version was not found.</exception>
        /// <exception cref="InvalidNapackException">If the retrieved Napack is invalid and cannot be deserialized.</exception>
        Task<NapackVersion> GetMostRecentMajorVersionAsync(NapackMajorVersion partialNapackVersionDefinition);

        /// <summary>
        /// Registers a user with the Napack Framework System.
        /// </summary>
        /// <param name="userEmail">The email to register.</param>
        /// <returns>The secrets that have been assigned to the user.</returns>
        Task<UserSecret> RegisterUserAsync(string userEmail);

        /// <summary>
        /// Creates a new Napack package.
        /// </summary>
        /// <param name="packageName">The name of the package to create.</param>
        /// <param name="newNapack">The new napack to create.</param>
        /// <returns>The operational results (success).</returns>
        Task<string> CreatePackageAsync(string packageName, NewNapack newNapack);

        /// <summary>
        /// Updates an existing Napack package.
        /// </summary>
        /// <param name="packageName">The name of the package to update.</param>
        /// <param name="newNapackVersion">The package data from which to use to create a new napack.</param>
        /// <returns>The new Napack version created.</returns>
        Task<VersionDescriptor> UpdatePackageAsync(string packageName, NewNapackVersion newNapackVersion);
    }
}