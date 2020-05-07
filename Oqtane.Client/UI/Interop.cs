using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
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
                _jsRuntime.InvokeAsync<string>(
                "interop.setCookie",
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
                    "interop.getCookie",
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
                _jsRuntime.InvokeAsync<string>(
                    "interop.updateTitle",
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
                _jsRuntime.InvokeAsync<string>(
                    "interop.includeMeta",
                    id, attribute, name, content);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task IncludeLink(string id, string rel, string url, string type, string integrity, string crossorigin)
        {
            try
            {
                _jsRuntime.InvokeAsync<string>(
                    "interop.includeLink",
                    id, rel, url, type, integrity, crossorigin);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task IncludeScript(string id, string src, string content, string location, string integrity, string crossorigin)
        {
            try
            {
                _jsRuntime.InvokeAsync<string>(
                    "interop.includeScript",
                    id, src, content, location, integrity, crossorigin);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task IncludeCSS(string id, string url)
        {
            try
            {
                _jsRuntime.InvokeAsync<string>(
                    "interop.includeLink",
                    id, "stylesheet", url, "text/css");
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
                    "interop.getElementByName",
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
                _jsRuntime.InvokeAsync<string>(
                "interop.submitForm",
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
                    "interop.getFiles",
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
                _jsRuntime.InvokeAsync<string>(
                "interop.uploadFiles",
                posturl, folder, id);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }
    }
}
