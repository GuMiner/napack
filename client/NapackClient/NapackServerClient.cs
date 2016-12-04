using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Napack.Common;

namespace Napack.Client
{
    public class NapackServerClient : INapackServerClient, IDisposable
    {
        private HttpClient client;

        public NapackServerClient(Uri napackFrameworkServer)
        {
            client = new HttpClient()
            {
                BaseAddress = napackFrameworkServer
            };
        }

        public void Dispose()
        {
            client?.Dispose();
            client = null;
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
        public async Task<NapackVersion> GetNapackVersionAsync(NapackVersionIdentifier napackVersionDefinition)
        {
            string responseContent = await this.GetWithCommonExceptionHandlingAsync("/napackDownload/" + napackVersionDefinition.GetDirectoryName(),
                napackVersionDefinition.NapackName, napackVersionDefinition.Major, napackVersionDefinition.Minor, napackVersionDefinition.Patch);
            return DeserializeNapack(responseContent);
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
        public async Task<NapackVersion> GetMostRecentMajorVersionAsync(NapackMajorVersion partialNapackVersionDefinition)
        {
            string responseContent = await this.GetWithCommonExceptionHandlingAsync("/dependency/" + partialNapackVersionDefinition.Name + "." + partialNapackVersionDefinition.Major,
                partialNapackVersionDefinition.Name, partialNapackVersionDefinition.Major);
            return DeserializeNapack(responseContent);
        }

        private async Task<string> GetWithCommonExceptionHandlingAsync(string uriSuffix, string napackName, int major, int? minor = null, int? patch = null)
        {
            HttpResponseMessage response = null;
            string responseContent = null;
            try
            {
                response = await client.GetAsync(uriSuffix);
                responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
            }
            catch (Exception ex)
            {
                throw new NapackFrameworkServerUnavailable(ex.Message);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                throw new NapackRecalledException(napackName, major);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NapackVersionNotFoundException(major, minor, patch);
            }
            else if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new NapackFrameworkServerUnavailable("Did not understand the response code from the server: " + response.StatusCode + ": " + responseContent);
            }

            response?.Dispose();
            return responseContent;
        }

        private NapackVersion DeserializeNapack(string responseContent)
        {
            try
            {
                NapackVersion napack = Serializer.Deserialize<NapackVersion>(responseContent);
                if (napack == null)
                {
                    throw new InvalidNapackException();
                }

                return napack;
            }
            catch (Exception ex)
            {
                throw new InvalidNapackException(ex.Message);
            }
        }
    }
}
