using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Services
{
    public class ServiceBase
    {
        private readonly HttpClient _http;

        protected ServiceBase(HttpClient client)
        {
            _http = client;
        }

        protected async Task<T> PutJsonAsync<T>(string uri, T value)
        {
            var response = await _http.PutAsJsonAsync(uri, value);
            var result = await response.Content.ReadFromJsonAsync<T>();
            return result;
        }

        protected async Task PutAsync(string uri)
        {
            await _http.PutAsync(uri, null);
        }

        protected async Task PostAsync(string uri)
        {
            await _http.PostAsync(uri, null);
        }

        protected async Task GetAsync(string uri)
        {
            await _http.GetAsync(uri);
        }

        protected async Task<byte[]> GetByteArrayAsync(string uri)
        {
            return await _http.GetByteArrayAsync(uri);
        }

        protected async Task<R> PostJsonAsync<T, R>(string uri, T value)
        {
            var response = await _http.PostAsJsonAsync(uri, value);
            if (!ValidateJsonContent(response.Content)) return default;
            
            var result = await response.Content.ReadFromJsonAsync<R>();
            return result;
        }

        private static bool ValidateJsonContent(HttpContent content)
        {
            var mediaType = content?.Headers.ContentType?.MediaType;
            return mediaType != null && mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
        }
  
        protected async Task<T> PostJsonAsync<T>(string uri, T value)
        {
            return await PostJsonAsync<T, T>(uri, value);
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

        private bool CheckResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return true;
            if (response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.NotFound)
            {
                //TODO: Log error here
            }

            return false;
        }

        protected async Task DeleteAsync(string uri)
        {
            await _http.DeleteAsync(uri);
        }

        protected async Task<string> GetStringAsync(string uri)
        {
            return await _http.GetStringAsync(uri);
        }

        public static string CreateApiUrl(Alias alias, string absoluteUri, string serviceName)
        {
            Uri uri = new Uri(absoluteUri);

            string apiurl;
            if (alias != null)
            {
                // build a url which passes the alias that may include a subfolder for multi-tenancy
                apiurl = $"{uri.Scheme}://{alias.Name}/";
                if (alias.Path == string.Empty)
                {
                    apiurl += "~/";
                }
            }
            else
            {
                // build a url which ignores any subfolder for multi-tenancy
                apiurl = $"{uri.Scheme}://{uri.Authority}/~/";
            }

            apiurl += $"api/{serviceName}";

            return apiurl;
        }

        public static string CreateCrossTenantUrl(string url, Alias alias)
        {
            if (alias != null)
            {
                url += (url.Contains("?")) ? "&" : "?";
                url += "aliasid=" + alias.AliasId.ToString();
            }

            return url;
        }
    }
}
