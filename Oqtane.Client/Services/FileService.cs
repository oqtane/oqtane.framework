using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class FileService : ServiceBase, IFileService
    {
        private readonly SiteState sitestate;
        private readonly IUriHelper urihelper;
        private readonly IJSRuntime jsRuntime;

        public FileService(SiteState sitestate, IUriHelper urihelper, IJSRuntime jsRuntime)
        {
            this.sitestate = sitestate;
            this.urihelper = urihelper;
            this.jsRuntime = jsRuntime;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, urihelper.GetAbsoluteUri(), "File"); }
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
