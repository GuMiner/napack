using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Napack.Common
{
    public class RestClient : IDisposable
    {
        public const string JsonMediaType = "application/json";

        private HttpClient client;

        public RestClient(Uri napackFrameworkServer)
        {
            client = new HttpClient()
            {
                BaseAddress = napackFrameworkServer
            };
        }

        public async Task<TResponse> PatchAsync<TResponse, TBody>(string uriSuffix, TBody body, Dictionary<HttpStatusCode, Exception> exceptionMap = null)
            where TResponse : class
        {
            string serializedContent = typeof(TBody) == typeof(string) ? body as string : Serializer.Serialize(body);
            using (StringContent content = new StringContent(serializedContent, Encoding.UTF8, RestClient.JsonMediaType))
            {
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), uriSuffix)
                {
                    Content = content
                })
                {
                    return await this.SendWithExceptionHandlingAndSerialization<TResponse>(() => client.SendAsync(requestMessage), exceptionMap);
                }
            }
        }

        public async Task<TResponse> PutAsync<TResponse, TBody>(string uriSuffix, TBody body, UserSecret userSecret)
            where TResponse : class
        {
            string serializedContent = typeof(TBody) == typeof(string) ? body as string : Serializer.Serialize(body);
            using (StringContent content = new StringContent(serializedContent, Encoding.UTF8, RestClient.JsonMediaType))
            {
                content.Headers.Add(CommonHeaders.UserId, userSecret.UserId);
                content.Headers.Add(CommonHeaders.UserKeys, Convert.ToBase64String(Encoding.UTF8.GetBytes(Serializer.Serialize(userSecret.Secrets))));content.Headers.Add(CommonHeaders.UserId, userSecret.UserId);
                content.Headers.Add(CommonHeaders.UserKeys, Convert.ToBase64String(Encoding.UTF8.GetBytes(Serializer.Serialize(userSecret.Secrets))));
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, uriSuffix)
                {
                    Content = content
                })
                {
                    return await this.SendWithExceptionHandlingAndSerialization<TResponse>(() => client.SendAsync(requestMessage));
                }
            }
        }

        public async Task<TResponse> PatchAsync<TResponse, TBody>(string uriSuffix, TBody body, UserSecret userSecret, Dictionary<HttpStatusCode, Exception> exceptionMap = null)
            where TResponse : class
        {
            string serializedContent = typeof(TBody) == typeof(string) ? body as string : Serializer.Serialize(body);
            using (StringContent content = new StringContent(serializedContent, Encoding.UTF8, RestClient.JsonMediaType))
            {
                content.Headers.Add(CommonHeaders.UserId, userSecret.UserId);
                content.Headers.Add(CommonHeaders.UserKeys, Convert.ToBase64String(Encoding.UTF8.GetBytes(Serializer.Serialize(userSecret.Secrets))));
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), uriSuffix)
                {
                    Content = content
                })
                {
                    return await this.SendWithExceptionHandlingAndSerialization<TResponse>(() => client.SendAsync(requestMessage), exceptionMap);
                }
            }
        }

        public async Task<TResponse> PostAsync<TResponse, TBody>(string uriSuffix, TBody body, UserSecret userSecret, Dictionary<HttpStatusCode, Exception> exceptionMap = null)
            where TResponse : class
        {
            string serializedContent = typeof(TBody) == typeof(string) ? body as string : Serializer.Serialize(body);
            using (StringContent content = new StringContent(serializedContent, Encoding.UTF8, RestClient.JsonMediaType))
            {
                content.Headers.Add(CommonHeaders.UserId, userSecret.UserId);
                content.Headers.Add(CommonHeaders.UserKeys, Convert.ToBase64String(Encoding.UTF8.GetBytes(Serializer.Serialize(userSecret.Secrets))));
                return await this.SendWithExceptionHandlingAndSerialization<TResponse>(() => client.PostAsync(uriSuffix, content), exceptionMap);
            }
        }

        public async Task<TResponse> PostAsync<TResponse, TBody>(string uriSuffix, TBody body, Dictionary<HttpStatusCode, Exception> exceptionMap = null)
            where TResponse: class
        {
            string serializedContent = typeof(TBody) == typeof(string) ? body as string : Serializer.Serialize(body);
            using (StringContent content = new StringContent(serializedContent, Encoding.UTF8, RestClient.JsonMediaType))
            {
                return await this.SendWithExceptionHandlingAndSerialization<TResponse>(() => client.PostAsync(uriSuffix, content), exceptionMap);
            }
        }

        public Task<T> GetAsync<T>(string uriSuffix, Dictionary<HttpStatusCode, Exception> exceptionMap = null)
            where T : class
        {
            return this.SendWithExceptionHandlingAndSerialization<T>(() => client.GetAsync(uriSuffix), exceptionMap);
        }

        public virtual void Dispose()
        {
            client?.Dispose();
            client = null;
        }

        private async Task<T> SendWithExceptionHandlingAndSerialization<T>(Func<Task<HttpResponseMessage>> sendAction, Dictionary<HttpStatusCode, Exception> exceptionMap = null)
            where T: class
        {
            HttpResponseMessage response = null;
            string responseContent = null;

            try
            {
                response = await sendAction().ConfigureAwait(false);
                responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
            }
            catch (Exception ex)
            {
                throw new NapackFrameworkServerUnavailable(ex.Message);
            }

            // Map the exception to a handled exception or throw a NFS Unavailable exception.
            if (!response.IsSuccessStatusCode)
            {
                exceptionMap = exceptionMap ?? new Dictionary<HttpStatusCode, Exception>();
                Exception error = null;
                if (exceptionMap.TryGetValue(response.StatusCode, out error))
                {
                    Console.WriteLine(responseContent); // TODO we should be passing in a function instead of a dictionary to avoid creating the errors unless we need them, and so we can include this text.
                    throw error;
                }
                else
                {
                    throw new NapackFrameworkServerUnavailable("Did not understand the response code from the server: " + response.StatusCode + ": " + responseContent);
                }
            }

            response?.Dispose();
            if (typeof(T) == typeof(string))
            {
                return responseContent as T;
            }

            return Serializer.Deserialize<T>(responseContent);
        }
    }
}
