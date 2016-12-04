using System.Threading.Tasks;
using Napack.Common;

namespace NapackClient
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
        Task<NapackVersion> GetNapackVersionAsync(DefinedNapackVersion napackVersionDefinition);

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
    }
}