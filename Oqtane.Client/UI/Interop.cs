using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.UI
{
    public class Interop
    {
        private readonly IJSRuntime _jsRuntime;

        public Interop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Task SetCookie(string name, string value, int days)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.setCookie",
                    name, value, days);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public ValueTask<string> GetCookie(string name)
        {
            try
            {
                return _jsRuntime.InvokeAsync<string>(
                    "Oqtane.Interop.getCookie",
                    name);
            }
            catch
            {
                return new ValueTask<string>(Task.FromResult(string.Empty));
            }
        }

        public Task UpdateTitle(string title)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.updateTitle",
                    title);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task IncludeMeta(string id, string attribute, string name, string content)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.includeMeta",
                    id, attribute, name, content);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task IncludeLink(string id, string rel, string href, string type, string integrity, string crossorigin, string includebefore)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.includeLink",
                    id, rel, href, type, integrity, crossorigin, includebefore);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task IncludeLinks(object[] links)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.includeLinks", 
                    (object) links);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        // external scripts need to specify src, inline scripts need to specify id and content
        public Task IncludeScript(string id, string src, string integrity, string crossorigin, string content, string location)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.includeScript",
                    id, src, integrity, crossorigin, content, location);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public async Task IncludeScripts(object[] scripts)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.includeScripts",
                    (object)scripts);
            }
            catch
            {
                // ignore exception
            }
        }

        public Task RemoveElementsById(string prefix, string first, string last)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.removeElementsById",
                    prefix, first, last);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public ValueTask<string> GetElementByName(string name)
        {
            try
            {
                return _jsRuntime.InvokeAsync<string>(
                    "Oqtane.Interop.getElementByName",
                    name);
            }
            catch
            {
                return new ValueTask<string>(Task.FromResult(string.Empty));
            }
        }

        public Task SubmitForm(string path, object fields)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.submitForm",
                    path, fields);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public ValueTask<string[]> GetFiles(string id)
        {
            try
            {
                return _jsRuntime.InvokeAsync<string[]>(
                    "Oqtane.Interop.getFiles",
                    id);
            }
            catch
            {
                return new ValueTask<string[]>(Task.FromResult(new string[0]));
            }
        }

        public Task UploadFiles(string posturl, string folder, string id, string antiforgerytoken)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.uploadFiles",
                    posturl, folder, id, antiforgerytoken);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task RefreshBrowser(bool force, int wait)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.refreshBrowser",
                    force, wait);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task RedirectBrowser(string url, int wait)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.redirectBrowser",
                    url, wait);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public ValueTask<bool> FormValid(ElementReference form)
        {
            try
            {
                return _jsRuntime.InvokeAsync<bool>(
                    "Oqtane.Interop.formValid",
                    form);
            }
            catch
            {
                return new ValueTask<bool>(Task.FromResult(false));
            }
        }

        public Task SetElementAttribute(string id, string attribute, string value)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.setElementAttribute",
                    id, attribute, value);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task ScrollTo(int top, int left, string behavior)
        {
            try
            {
                if (string.IsNullOrEmpty(behavior)) behavior = "smooth";
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.scrollTo",
                    top, left, behavior);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task ScrollToId(string id)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.scrollToId",
                    id);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public ValueTask<int> GetCaretPosition(string id)
        {
            try
            {
                return _jsRuntime.InvokeAsync<int>(
                    "Oqtane.Interop.getCaretPosition",
                    id);
            }
            catch
            {
                return new ValueTask<int>(-1);
            }
        }

        public Task SetIndexedDBItem(string key, object value)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.manageIndexedDBItems",
                    "put", key, value);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public async Task<T> GetIndexedDBItem<T>(string key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<T>(
                    "Oqtane.Interop.manageIndexedDBItems",
                    "get", key, null);
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<List<string>> GetIndexedDBKeys()
        {
            return await GetIndexedDBKeys("");
        }

        public async Task<List<string>> GetIndexedDBKeys(string contains)
        {
            try
            {
                var items = await _jsRuntime.InvokeAsync<JsonDocument>(
                    "Oqtane.Interop.manageIndexedDBItems",
                    "getallkeys", null, null);
                if (!string.IsNullOrEmpty(contains))
                {
                    return items.Deserialize<List<string>>()
                        .Where(item => item.Contains(contains)).ToList();
                }
                else
                {
                    return items.Deserialize<List<string>>();
                }
            }
            catch
            {
                return new List<string>();
            }
        }

        public Task RemoveIndexedDBItem(string key)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.manageIndexedDBItems",
                    "delete", key, null);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }
    }
}
