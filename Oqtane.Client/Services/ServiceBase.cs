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
