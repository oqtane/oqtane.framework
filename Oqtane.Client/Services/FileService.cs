using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class FileService : ServiceBase, IFileService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;
        private readonly IJSRuntime jsRuntime;

        public FileService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager, IJSRuntime jsRuntime)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
            this.jsRuntime = jsRuntime;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "File"); }
        }

        public async Task<List<string>> GetFilesAsync(string Folder)
        {
            return await http.GetJsonAsync<List<string>>(apiurl + "?folder=" + Folder);
        }

        public async Task<bool> UploadFilesAsync(string Folder, string[] Files, string FileUploadName)
        {
            bool success = false;
            var interop = new Interop(jsRuntime);
            await interop.UploadFiles(apiurl + "/upload", Folder, FileUploadName);
            List<string> files = await GetFilesAsync(Folder);
            if (files.Count > 0)
            {
                success = true;
                foreach (string file in Files)
                {
                    if (!files.Contains(file))
                    {
                        success = false;
                    }
                }
            }
            return success;
        }
    }
}
