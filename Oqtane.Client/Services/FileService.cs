using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Models;
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

        public async Task<List<File>> GetFilesAsync(int FolderId)
        {
            return await GetFilesAsync(FolderId.ToString());
        }

        public async Task<List<File>> GetFilesAsync(string Folder)
        {
            return await http.GetJsonAsync<List<File>>(apiurl + "?folder=" + Folder);
        }

        public async Task<File> GetFileAsync(int FileId)
        {
            return await http.GetJsonAsync<File>(apiurl + "/" + FileId.ToString());
        }

        public async Task<File> AddFileAsync(File File)
        {
            return await http.PostJsonAsync<File>(apiurl, File);
        }

        public async Task<File> UpdateFileAsync(File File)
        {
            return await http.PutJsonAsync<File>(apiurl + "/" + File.FileId.ToString(), File);
        }

        public async Task DeleteFileAsync(int FileId)
        {
            await http.DeleteAsync(apiurl + "/" + FileId.ToString());
        }

        public async Task<File> UploadFileAsync(string Url, int FolderId)
        {
            return await http.GetJsonAsync<File>(apiurl + "/upload?url=" + WebUtility.UrlEncode(Url) + "&folderid=" + FolderId.ToString());
        }

        public async Task<string> UploadFilesAsync(int FolderId, string[] Files, string Id)
        {
            return await UploadFilesAsync(FolderId.ToString(), Files, Id);
        }

        public async Task<string> UploadFilesAsync(string Folder, string[] Files, string Id)
        {
            string result = "";

            var interop = new Interop(jsRuntime);
            await interop.UploadFiles(apiurl + "/upload", Folder, Id);

            // uploading files is asynchronous so we need to wait for the upload to complete
            bool success = false;
            int attempts = 0;
            while (attempts < 5 && success == false)
            {
                Thread.Sleep(2000); // wait 2 seconds
                result = "";

                List<File> files = await GetFilesAsync(Folder);
                if (files.Count > 0)
                {
                    success = true;
                    foreach (string file in Files)
                    {
                        if (!files.Exists(item => item.Name == file))
                        {
                            success = false;
                            result += file + ",";
                        }
                    }
                }
                attempts += 1;
            }
            if (!success)
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }

        public async Task<byte[]> DownloadFileAsync(int FileId)
        {
            return await http.GetByteArrayAsync(apiurl + "/download/" + FileId.ToString());
        }
    }
}
