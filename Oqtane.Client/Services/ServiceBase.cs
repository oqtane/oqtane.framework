using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public class ServiceBase
    {
        private readonly HttpClient _http;

        protected ServiceBase(HttpClient client)
        {
            _http = client;
        }

        protected async Task GetAsync(string uri)
        {
            var response = await _http.GetAsync(uri);
            CheckResponse(response);
        }

        protected async Task<string> GetStringAsync(string uri)
        {
            try
            {
                return await _http.GetStringAsync(uri);
            }
            catch (Exception e)
            {
                //TODO replace with logging
                Console.WriteLine(e);
            }

            return default;
        }

        protected async Task<byte[]> GetByteArrayAsync(string uri)
        {
            try
            {
                return await _http.GetByteArrayAsync(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default;
        }

        protected async Task<T> GetJsonAsync<T>(string uri)
        {
            var response = await _http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);
            if (CheckResponse(response) && ValidateJsonContent(response.Content))
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            return default;
        }

        protected async Task PutAsync(string uri)
        {
            var response = await _http.PutAsync(uri, null);
            CheckResponse(response);
        }

        protected async Task<T> PutJsonAsync<T>(string uri, T value)
        {
            return await PutJsonAsync<T, T>(uri, value);
        }

        protected async Task<TResult> PutJsonAsync<TValue, TResult>(string uri, TValue value)
        {
            var response = await _http.PutAsJsonAsync(uri, value);
            if (CheckResponse(response) && ValidateJsonContent(response.Content))
            {
                var result = await response.Content.ReadFromJsonAsync<TResult>();
                return result;
            }
            return default;
        }

        protected async Task PostAsync(string uri)
        {
            var response = await _http.PostAsync(uri, null);
            CheckResponse(response);
        }

        protected async Task<T> PostJsonAsync<T>(string uri, T value)
        {
            return await PostJsonAsync<T, T>(uri, value);
        }

        protected async Task<TResult> PostJsonAsync<TValue, TResult>(string uri, TValue value)
        {
            var response = await _http.PostAsJsonAsync(uri, value);
            if (CheckResponse(response) && ValidateJsonContent(response.Content))
            {
                var result = await response.Content.ReadFromJsonAsync<TResult>();
                return result;
            }

            return default;
        }

        protected async Task DeleteAsync(string uri)
        {
            var response = await _http.DeleteAsync(uri);
            CheckResponse(response);
        }

        private bool CheckResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return true;
            if (response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.NotFound)
            {
                //TODO: Log errors here
                
                Console.WriteLine($"Request: {response.RequestMessage.RequestUri}");
                Console.WriteLine($"Response status: {response.StatusCode} {response.ReasonPhrase}");
            }

            return false;
        }

        private static bool ValidateJsonContent(HttpContent content)
        {
            var mediaType = content?.Headers.ContentType?.MediaType;
            return mediaType != null && mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
            //TODO Missing content JSON validation 
        }

        // create an API Url which is tenant agnostic ( for use during installation )
        public string CreateApiUrl(string serviceName)
        {
            return CreateApiUrl(null, serviceName);
        }

        // create an API Url which is tenant aware ( for use with repositories )
        public string CreateApiUrl(Alias alias, string serviceName)
        {
            string apiurl = "/";

            if (Alias != null)
            {
                alias = Alias; // override the default alias ( for cross-tenant service calls )
            }

            if (alias != null)
            {
                // include the alias for multi-tenant context
                apiurl += $"{alias.AliasId}/";
            }
            else
            {
                // tenant agnostic
                apiurl += "~/";
            }

            apiurl += $"api/{serviceName}";

            return apiurl;
        }

        // can be used to override the default alias
        public Alias Alias { get; set; }

        // add entityid parameter to url for custom authorization policy
        public string CreateAuthorizationPolicyUrl(string url, int entityId)
        {
            if (url.Contains("?"))
            {
                return url + "&entityid=" + entityId.ToString();
            }
            else
            {
                return url + "?entityid=" + entityId.ToString();
            }
        }

        [Obsolete("This method is obsolete. Use CreateApiUrl(Alias alias, string serviceName) instead.", false)]
        public string CreateApiUrl(Alias alias, string absoluteUri, string serviceName)
        {
            // only retained for short term backward compatibility
            return CreateApiUrl(alias, serviceName);
        }
    }
}
