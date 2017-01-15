using System;
using System.Collections.Generic;
using System.Net;
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

        public Task<string> VerifyUserAsync(string userEmail, Guid verificationCode)
        {
            return this.PatchAsync<string, object>("/users", new
            {
                Email = userEmail,
                EmailVerificationCode = verificationCode
            });
        }

        public Task<string> CreatePackageAsync(string packageName, NewNapack newNapack, UserSecret userSecret)
        {
            return this.PostAsync<string, NewNapack>("/napacks/" + packageName, newNapack, userSecret,
                new Dictionary<HttpStatusCode, Exception>
                {
                    [HttpStatusCode.Conflict] = new DuplicateNapackException(),
                    [HttpStatusCode.BadRequest] = new InvalidNapackException("The napack contents were invalid!")
                });
        }

        public Task<VersionDescriptor> UpdatePackageAsync(string packageName, NewNapackVersion newNapackVersion, UserSecret userSecret)
        {
            return this.PatchAsync<VersionDescriptor, NewNapackVersion>("/napacks/" + packageName, newNapackVersion, userSecret,
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
        /// Returns true if the specified Napack exists, false otherwise.
        /// </summary>
        public async Task<bool> ContainsNapack(string packageName)
        {
            try
            {
                await this.GetAsync<string>("/napacks/" + packageName, new Dictionary<HttpStatusCode, Exception>
                {
                    [HttpStatusCode.NotFound] = new NapackNotFoundException(packageName)
                }).ConfigureAwait(false);
                return true;
            }
            catch (NapackNotFoundException)
            {
                return false;
            }
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
