using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ServiceBase
    {
        private readonly HttpClient _httpClient;
        private readonly SiteState _siteState;

        protected ServiceBase(HttpClient httpClient, SiteState siteState)
        {
            _httpClient = httpClient;
            _siteState = siteState;
        }

        private HttpClient GetHttpClient()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains(Constants.AntiForgeryTokenHeaderName) && _siteState != null && !string.IsNullOrEmpty(_siteState.AntiForgeryToken))
            {
                _httpClient.DefaultRequestHeaders.Add(Constants.AntiForgeryTokenHeaderName, _siteState.AntiForgeryToken);
            }
            return _httpClient;
        }

        // should be used with new constructor
        public string CreateApiUrl(string serviceName)
        {
            if (_siteState != null)
            {
                return CreateApiUrl(serviceName, _siteState.Alias, ControllerRoutes.ApiRoute);
            }
            else // legacy support (before 2.1.0)
            {           
                return CreateApiUrl(serviceName, null, ControllerRoutes.Default);
            }
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

        public string CreateAuthorizationPolicyUrl(string url, Dictionary<string, int> authEntityId)
        {
            string qs = "";
            foreach (KeyValuePair<string, int> kvp in authEntityId)
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
            var response = await GetHttpClient().GetAsync(uri);
            CheckResponse(response);
        }

        protected async Task<string> GetStringAsync(string uri)
        {
            try
            {
                return await GetHttpClient().GetStringAsync(uri);
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
                return await GetHttpClient().GetByteArrayAsync(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default;
        }

        protected async Task<T> GetJsonAsync<T>(string uri)
        {
            var response = await GetHttpClient().GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);
            if (CheckResponse(response) && ValidateJsonContent(response.Content))
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            return default;
        }

        protected async Task PutAsync(string uri)
        {
            var response = await GetHttpClient().PutAsync(uri, null);
            CheckResponse(response);
        }

        protected async Task<T> PutJsonAsync<T>(string uri, T value)
        {
            return await PutJsonAsync<T, T>(uri, value);
        }

        protected async Task<TResult> PutJsonAsync<TValue, TResult>(string uri, TValue value)
        {
            var response = await GetHttpClient().PutAsJsonAsync(uri, value);
            if (CheckResponse(response) && ValidateJsonContent(response.Content))
            {
                var result = await response.Content.ReadFromJsonAsync<TResult>();
                return result;
            }
            return default;
        }

        protected async Task PostAsync(string uri)
        {
            var response = await GetHttpClient().PostAsync(uri, null);
            CheckResponse(response);
        }

        protected async Task<T> PostJsonAsync<T>(string uri, T value)
        {
            return await PostJsonAsync<T, T>(uri, value);
        }

        protected async Task<TResult> PostJsonAsync<TValue, TResult>(string uri, TValue value)
        {
            var response = await GetHttpClient().PostAsJsonAsync(uri, value);
            if (CheckResponse(response) && ValidateJsonContent(response.Content))
            {
                var result = await response.Content.ReadFromJsonAsync<TResult>();
                return result;
            }

            return default;
        }

        protected async Task DeleteAsync(string uri)
        {
            var response = await GetHttpClient().DeleteAsync(uri);
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

        //[Obsolete("This constructor is obsolete. Use ServiceBase(HttpClient client, SiteState siteState) : base(http, siteState) {} instead.", false)]
        // This constructor is obsolete. Use ServiceBase(HttpClient client, SiteState siteState) : base(http, siteState) {} instead.
        protected ServiceBase(HttpClient client)
        {
            _httpClient = client;
        }

        [Obsolete("This method is obsolete. Use CreateApiUrl(string serviceName, Alias alias) in conjunction with ControllerRoutes.ApiRoute in Controllers instead.", false)]
        public string CreateApiUrl(Alias alias, string serviceName)
        {
            return CreateApiUrl(serviceName, alias, ControllerRoutes.Default);
        }

        [Obsolete("This property of ServiceBase is deprecated. Cross tenant service calls are not supported.", false)]
        public Alias Alias { get; set; }

        [Obsolete("This method is obsolete. Use CreateAuthorizationPolicyUrl(string url, string entityName, int entityId) where entityName = EntityNames.Module instead.", false)]
        public string CreateAuthorizationPolicyUrl(string url, int entityId)
        {
            return url + ((url.Contains("?")) ? "&" : "?") + "entityid=" + entityId.ToString();
        }
    }
}
