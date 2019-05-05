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

        public Task<string> SetCookie(string name, string value, int days)
        {
            try
            {
                return jsRuntime.InvokeAsync<string>(
                "interop.setCookie",
                name, value, days);
            }
            catch
            {
                return Task.FromResult(string.Empty);
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

        public Task<string> AddCSS(string filename)
        {
            try
            {
                return jsRuntime.InvokeAsync<string>(
                    "interop.addCSS",
                    filename);
            }
            catch
            {
                return Task.FromResult(string.Empty);
            }
        }
    }
}
