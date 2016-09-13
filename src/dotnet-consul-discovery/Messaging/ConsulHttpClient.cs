using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace Consul.Discovery.Messaging
{
    /// <summary>
    /// Represents Consul HTTP client.
    /// </summary>
    public class ConsulHttpClient : IDisposable
    {
        private readonly Uri _baseUrl;
        private readonly HttpClient _client;
        
        /// <summary>
        /// Gets the underlying HTTP client used to communicate via.
        /// </summary>
        protected HttpClient Client
        {
            get { return _client; }
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        protected Uri BaseUrl
        {
            get { return _baseUrl; }
        }

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        /// <param name="origin">URL origin. For example, "http://localhost:5000".</param>
        /// <exception cref="ArgumentException">Occurs when <paramref name="origin" /> is either null or an empty string.</exception> 
        public ConsulHttpClient(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                throw new ArgumentException("URL origin cannot be an empty string.", "origin");
            }
            
            _baseUrl = new Uri(origin);
            _client = new HttpClient();
        }

        /// <summary>
        /// Issues HTTP "GET" request.
        /// </summary>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="relativeUrl">Relative URL. For example, "catalog/services".</param>
        /// <returns>A task which, when completes, returns the response.</returns>
        public async Task<string> Get(string relativeUrl)
        {
            return await Send(HttpMethod.Get, relativeUrl);
        }

        /// <summary>
        /// Issues HTTP "POST" request.
        /// </summary>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="relativeUrl">Relative URL. For example, "catalog/services".</param>
        /// <param name="requestBody">Request body.</param>
        /// <returns>A task which, when completes, returns the response.</returns>
        public async Task<string> Post(string relativeUrl, object requestBody)
        {
            return await Send(HttpMethod.Post, relativeUrl, requestBody);
        }

        /// <summary>
        /// Issues HTTP "PUT" request.
        /// </summary>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="relativeUrl">Relative URL. For example, "catalog/services".</param>
        /// <param name="requestBody">Request body.</param>
        /// <returns>A task which, when completes, returns the response.</returns>
        public async Task<string> Put(string relativeUrl, object requestBody)
        {
            return await Send(HttpMethod.Put, relativeUrl, requestBody);
        }

        /// <summary>
        /// Issues HTTP "DELETE" request.
        /// </summary>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="relativeUrl">Relative URL. For example, "catalog/services".</param>
        /// <returns>A task which, when completes, returns the response.</returns>
        public async Task<string> Delete(string relativeUrl)
        {
            return await Send(HttpMethod.Delete, relativeUrl);
        }

        /// <summary>
        /// Deserializes content to a strongly-typed representation.
        /// </summary>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="content">Content.</param>
        /// <returns>Deserialized content.</returns>
        public TResult DeserializeContent<TResult>(string content)
        {
            return JsonConvert.DeserializeObject<TResult>(content);
        }

        /// <summary>
        /// Release unmanaged resources and disposes managed resources used by underlying HTTP client.
        /// </summary>
        public virtual void Dispose()
        {
            Client.Dispose();
        }

        /// <summary>
        /// Sends an HTTP request.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="relativeUrl">Relative URL. For example, "catalog/services".</param>
        /// <param name="body">Request body (optional).</param>
        /// <returns>A task which, when completes, returns the response.</returns>
        /// <exception cref="HttpRequestException">Occurs in case of non-successful response.</exception>
        protected virtual async Task<string> Send(HttpMethod method, string relativeUrl, object body = null)
        {
            var request = CreateRequest(method, relativeUrl, body);
            var response = await Client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    string.Concat(
                        "An error occured while calling Consul REST API. ",
                        "Status code: ",
                        response.StatusCode.ToString(),
                        ", Response body: ",
                        responseBody
                    )
                );
            }

            return responseBody;
        }

        /// <summary>
        /// Creates and returns HTTP request message.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="relativeUrl">Relative URL.</param>
        /// <param name="body">Request body (optional).</param>
        /// <returns>HTTP request message.</returns>
        protected virtual HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl, object body = null)
        {
            var contentType = "application/json";
            var ret = new HttpRequestMessage(method, new Uri(BaseUrl, string.Concat("v1/", relativeUrl)));

            ret.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

            if (body != null)
            {
                ret.Content = new StringContent(
                    body is string ? body.ToString() : JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    contentType
                );
            }

            return ret;
        }
    }
}
