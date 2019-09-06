using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Oqtane.Shared
{
    public class Interop
    {
        private readonly IJSRuntime jsRuntime;

        public Interop(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public Task SetCookie(string name, string value, int days)
        {
            try
            {
                jsRuntime.InvokeAsync<string>(
                "interop.setCookie",
                name, value, days);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task<string> GetCookie(string name)
        {
            try
            {
                return jsRuntime.InvokeAsync<string>(
                    "interop.getCookie",
                    name);
            }
            catch
            {
                return Task.FromResult(string.Empty);
            }
        }

        public Task AddCSS(string filename)
        {
            try
            {
                jsRuntime.InvokeAsync<string>(
                    "interop.addCSS",
                    filename);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task<string> GetElementByName(string name)
        {
            try
            {
                return jsRuntime.InvokeAsync<string>(
                    "interop.getElementByName",
                    name);
            }
            catch
            {
                return Task.FromResult(string.Empty);
            }
        }

        public Task SubmitForm(string path, object fields)
        {
            try
            {
                jsRuntime.InvokeAsync<string>(
                "interop.submitForm",
                path, fields);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task UploadFiles(string posturl, string folder, string name)
        {
            try
            {
                jsRuntime.InvokeAsync<string>(
                "interop.uploadFiles",
                posturl, folder, name);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }
    }
}
