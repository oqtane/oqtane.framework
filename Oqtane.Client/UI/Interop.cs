using Microsoft.JSInterop;
using Oqtane.Models;
using System.Threading.Tasks;

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

        public Task IncludeMeta(string id, string attribute, string name, string content, string key)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.includeMeta",
                    id, attribute, name, content, key);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task IncludeLink(string id, string rel, string href, string type, string integrity, string crossorigin, string key)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.includeLink",
                    id, rel, href, type, integrity, crossorigin, key);
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

        public Task IncludeScript(string id, string src, string integrity, string crossorigin, string content, string location, string key)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.includeScript",
                    id, src, integrity, crossorigin, content, location, key);
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

        public Task UploadFiles(string posturl, string folder, string id)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.Interop.uploadFiles",
                    posturl, folder, id);
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

    }
}
