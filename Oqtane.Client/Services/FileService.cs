using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class FileService : ServiceBase, IFileService
    {
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;
        private readonly IJSRuntime jsRuntime;

        public FileService(SiteState sitestate, NavigationManager NavigationManager, IJSRuntime jsRuntime)
        {
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
            this.jsRuntime = jsRuntime;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "File"); }
        }

        public async Task UploadFilesAsync(string Folder)
        {
            await UploadFilesAsync(Folder, "");
        }

        public async Task UploadFilesAsync(string Folder, string FileUploadName)
        {
            var interop = new Interop(jsRuntime);
            await interop.UploadFiles(apiurl + "/upload", Folder, FileUploadName);
        }
    }
}
