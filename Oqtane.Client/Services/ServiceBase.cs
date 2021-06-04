using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ServiceBase
    {
        private readonly HttpClient _http;

        protected ServiceBase(HttpClient client)
        {
            _http = client;
        }

        public string CreateApiUrl(string serviceName, Alias alias)
        {
            return CreateApiUrl(serviceName, alias, ControllerRoutes.ApiRoute);
        }

        public string CreateApiUrl(string serviceName, Alias alias, string routeTemplate)
        {
            string apiurl = "/";
            if (routeTemplate == ControllerRoutes.ApiRoute)
            {
                if (alias != null && !string.IsNullOrEmpty(alias.Path))
                {
                    // include the alias path for multi-tenant context
                    apiurl += alias.Path + "/";
                }
            }
            else
            {
                // legacy support for ControllerRoutes.Default
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
            }
            apiurl += $"api/{serviceName}";
            return apiurl;
        }

        // add authentityid parameters to url for custom authorization policy
        public string CreateAuthorizationPolicyUrl(string url, string entityName, int entityId)
        {
            return CreateAuthorizationPolicyUrl(url, new Dictionary<string, int>() { { entityName, entityId } });
        }

        public string CreateAuthorizationPolicyUrl(string url, Dictionary<string, int> args)
        {
            string qs = "";
            foreach (KeyValuePair<string, int> kvp in args)
            {
                qs += (qs != "") ? "&" : "";
                qs += "auth" + kvp.Key.ToLower() + "id=" + kvp.Value.ToString();
            }

            if (url.Contains("?"))
            {
                return url + "&" + qs;
            }
            else
            {
                return url + "?" + qs;
            }
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
                Console.WriteLine($"Request: {response.RequestMessage.RequestUri}");
                Console.WriteLine($"Response status: {response.StatusCode} {response.ReasonPhrase}");
            }

            return false;
        }

        private static bool ValidateJsonContent(HttpContent content)
        {
            var mediaType = content?.Headers.ContentType?.MediaType;
            return mediaType != null && mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
        }

        [Obsolete("This method is obsolete. Use CreateApiUrl(string serviceName, Alias alias) in conjunction with ControllerRoutes.ApiRoute in Controllers instead.", false)]
        public string CreateApiUrl(string serviceName)
        {
            return CreateApiUrl(serviceName, null, ControllerRoutes.Default);
        }

        [Obsolete("This method is obsolete. Use CreateApiUrl(string serviceName, Alias alias) in conjunction with ControllerRoutes.ApiRoute in Controllers instead.", false)]
        public string CreateApiUrl(Alias alias, string serviceName)
        {
            return CreateApiUrl(serviceName, alias, ControllerRoutes.Default);
        }

        [Obsolete("This property of ServiceBase is deprecated. Cross tenant service calls are not supported.", false)]
        public Alias Alias { get; set; }

        [Obsolete("This method is obsolete. Use CreateApiUrl(string entityName, int entityId) instead.", false)]
        public string CreateAuthorizationPolicyUrl(string url, int entityId)
        {
            return url + ((url.Contains("?")) ? "&" : "?") + "entityid=" + entityId.ToString();
        }
    }
}
