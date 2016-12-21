using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Napack.Common;

namespace Napack.Client.Common
{
    public class NapackServerClient : INapackServerClient, IDisposable
    {
        private const string JsonMediaType = "application/json";
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
            string responseContent = await this.GetWithCommonExceptionHandlingAsync("/napackDownload/" + napackVersionDefinition.GetFullName(),
                napackVersionDefinition.NapackName, napackVersionDefinition.Major, napackVersionDefinition.Minor, napackVersionDefinition.Patch);
            return DeserializeNapack(responseContent);
        }

        public async Task<UserSecret> RegisterUserAsync(string userEmail)
        {
            string serializedEmail = Serializer.Serialize(new
            {
                Email = userEmail
            });

            using (StringContent content = new StringContent(serializedEmail, Encoding.UTF8, NapackServerClient.JsonMediaType))
            {
                HttpResponseMessage response = null;
                string responseContent = null;
                try
                {
                    response = await client.PostAsync("/users", content);
                    responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
                }
                catch (Exception ex)
                {
                    throw new NapackFrameworkServerUnavailable(ex.Message);
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new ExistingUserException(userEmail);
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new NapackFrameworkServerUnavailable("Did not understand the response code from the server: " + response.StatusCode + ": " + responseContent);
                }

                response?.Dispose();
                return Serializer.Deserialize<UserSecret>(responseContent);
            }
        }

        public async Task<string> CreatePackageAsync(string packageName, NewNapack newNapack)
        {
            using (StringContent content = new StringContent(Serializer.Serialize(newNapack), Encoding.UTF8, NapackServerClient.JsonMediaType))
            {
                HttpResponseMessage response = null;
                string responseContent = null;
                try
                {
                    response = await client.PostAsync("/napacks/" + packageName, content);
                    responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
                }
                catch (Exception ex)
                {
                    throw new NapackFrameworkServerUnavailable(ex.Message);
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new DuplicateNapackException();
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new InvalidNapackException("The napack contents were invalid: " + responseContent);
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new NapackFrameworkServerUnavailable("Did not understand the response code from the server: " + response.StatusCode + ": " + responseContent);
                }

                response?.Dispose();
                return responseContent;
            }
        }

        public async Task<VersionDescriptor> UpdatePackageAsync(string packageName, NewNapackVersion newNapackVersion)
        {
            using (StringContent content = new StringContent(Serializer.Serialize(newNapackVersion), Encoding.UTF8, NapackServerClient.JsonMediaType))
            {
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), "/napacks/" + packageName)
                {
                    Content = content
                })
                {
                    HttpResponseMessage response = null;
                    string responseContent = null;
                    try
                    {
                        response = await client.SendAsync(requestMessage);
                        responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
                    }
                    catch (Exception ex)
                    {
                        throw new NapackFrameworkServerUnavailable(ex.Message);
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new InvalidNapackException("The napack contents were invalid: " + responseContent);
                    }
                    else if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new NapackFrameworkServerUnavailable("Did not understand the response code from the server: " + response.StatusCode + ": " + responseContent);
                    }

                    response?.Dispose();
                    return Serializer.Deserialize<VersionDescriptor>(responseContent);
                }
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

            if (response.StatusCode == HttpStatusCode.Gone)
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
