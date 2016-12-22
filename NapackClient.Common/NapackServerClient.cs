using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Napack.Common;

namespace Napack.Client.Common
{
    public class NapackServerClient : RestClient, INapackServerClient
    {
        public NapackServerClient(Uri napackFrameworkServer)
            : base(napackFrameworkServer)
        { }


        public Task<UserSecret> RegisterUserAsync(string userEmail)
        {
            return this.PostAsync<UserSecret, object>("/users", new
                {
                    Email = userEmail
                },
                new Dictionary<HttpStatusCode, Exception>
                {
                    [HttpStatusCode.Conflict] = new ExistingUserException(userEmail)
                });
        }

        public Task<string> CreatePackageAsync(string packageName, NewNapack newNapack)
        {
            return this.PostAsync<string, NewNapack>("/napacks", newNapack,
                new Dictionary<HttpStatusCode, Exception>
                {
                    [HttpStatusCode.Conflict] = new DuplicateNapackException(),
                    [HttpStatusCode.BadRequest] = new InvalidNapackException("The napack contents were invalid!")
                });
        }

        public Task<VersionDescriptor> UpdatePackageAsync(string packageName, NewNapackVersion newNapackVersion)
        {
            return this.PatchAsync<VersionDescriptor, NewNapackVersion>("/napacks" + packageName, newNapackVersion,
                new Dictionary<HttpStatusCode, Exception>
                {
                    [HttpStatusCode.BadRequest] = new InvalidNapackException("The napack contents were invalid!")
                });
        }

        /// <summary>
        /// Retrieves a <see cref="NapackVersion"/> from the Napack Framework Server.
        /// </summary>
        /// <param name="napackVersionDefinition">The napack version to retrieve.</param>
        /// <returns>The <see cref="NapackVersion"/> retrieved.</returns>
        /// <exception cref="NapackFrameworkServerUnavailable">If the Napack Framework Server is unavailable.</exception>
        /// <exception cref="NapackRecalledException">If the Napack was found, but is no longer available for download.</exception>
        /// <exception cref="NapackVersionNotFoundException">If the specified Napack version was not found.</exception>
        /// <exception cref="InvalidNapackException">If the retrieved Napack is invalid and cannot be deserialized.</exception>
        public Task<NapackVersion> GetNapackVersionAsync(NapackVersionIdentifier napackVersionDefinition)
        {
            return this.GetWithCommonExceptionHandlingAsync<NapackVersion>("/napackDownload/" + napackVersionDefinition.GetFullName(),
                napackVersionDefinition.NapackName, napackVersionDefinition.Major, napackVersionDefinition.Minor, napackVersionDefinition.Patch);
        }

        /// <summary>
        /// Retrieves the most recent minor/patch version of a major version'd <see cref="NapackVersion"/> from the Napack Framework Server.
        /// </summary>
        /// <param name="partialNapackVersionDefinition">The partial napack version to retrieve.</param>
        /// <returns>The <see cref="NapackVersion"/> retrieved.</returns>
        /// <exception cref="NapackFrameworkServerUnavailable">If the Napack Framework Server is unavailable.</exception>
        /// <exception cref="NapackRecalledException">If the Napack was found, but is no longer available for download.</exception>
        /// <exception cref="NapackVersionNotFoundException">If the specified Napack version was not found.</exception>
        /// <exception cref="InvalidNapackException">If the retrieved Napack is invalid and cannot be deserialized.</exception>
        public Task<NapackVersion> GetMostRecentMajorVersionAsync(NapackMajorVersion partialNapackVersionDefinition)
        {
            return this.GetWithCommonExceptionHandlingAsync<NapackVersion>("/dependency/" + partialNapackVersionDefinition.Name + "." + partialNapackVersionDefinition.Major,
                partialNapackVersionDefinition.Name, partialNapackVersionDefinition.Major);
        }

        private Task<T> GetWithCommonExceptionHandlingAsync<T>(string uriSuffix, string napackName, int major, int? minor = null, int? patch = null)
            where T: class
        {
            return this.GetAsync<T>(uriSuffix, new Dictionary<HttpStatusCode, Exception>
            {
                [HttpStatusCode.Gone] = new NapackRecalledException(napackName, major),
                [HttpStatusCode.NotFound] = new NapackVersionNotFoundException(major, minor, patch)
            });
        }
    }
}
