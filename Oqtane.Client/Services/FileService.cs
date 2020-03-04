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
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public FileService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager, IJSRuntime jsRuntime)
        {
            this._http = http;
            this._siteState = sitestate;
            this._navigationManager = NavigationManager;
            this._jsRuntime = jsRuntime;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "File"); }
        }

        public async Task<List<File>> GetFilesAsync(int FolderId)
        {
            return await GetFilesAsync(FolderId.ToString());
        }

        public async Task<List<File>> GetFilesAsync(string Folder)
        {
            return await _http.GetJsonAsync<List<File>>(apiurl + "?folder=" + Folder);
        }

        public async Task<File> GetFileAsync(int FileId)
        {
            return await _http.GetJsonAsync<File>(apiurl + "/" + FileId.ToString());
        }

        public async Task<File> AddFileAsync(File File)
        {
            return await _http.PostJsonAsync<File>(apiurl, File);
        }

        public async Task<File> UpdateFileAsync(File File)
        {
            return await _http.PutJsonAsync<File>(apiurl + "/" + File.FileId.ToString(), File);
        }

        public async Task DeleteFileAsync(int FileId)
        {
            await _http.DeleteAsync(apiurl + "/" + FileId.ToString());
        }

        public async Task<File> UploadFileAsync(string Url, int FolderId)
        {
            return await _http.GetJsonAsync<File>(apiurl + "/upload?url=" + WebUtility.UrlEncode(Url) + "&folderid=" + FolderId.ToString());
        }

        public async Task<string> UploadFilesAsync(int FolderId, string[] Files, string Id)
        {
            return await UploadFilesAsync(FolderId.ToString(), Files, Id);
        }

        public async Task<string> UploadFilesAsync(string Folder, string[] Files, string Id)
        {
            string result = "";

            var interop = new Interop(_jsRuntime);
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
            return await _http.GetByteArrayAsync(apiurl + "/download/" + FileId.ToString());
        }
    }
}
