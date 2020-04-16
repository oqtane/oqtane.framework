using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane.Services
{
    public class FileService : ServiceBase, IFileService
    {
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public FileService(HttpClient http, SiteState siteState, NavigationManager navigationManager,
            IJSRuntime jsRuntime) : base(http)
        {
            _siteState = siteState;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "File"); }
        }

        public async Task<List<File>> GetFilesAsync(int folderId)
        {
            return await GetFilesAsync(folderId.ToString());
        }

        public async Task<List<File>> GetFilesAsync(string folder)
        {
            return await GetJsonAsync<List<File>>($"{Apiurl}?folder={folder}");
        }

        public async Task<List<File>> GetFilesAsync(int siteId, string folderPath)
        {
            if (!folderPath.EndsWith("\\"))
            {
                folderPath += "\\";
            }
            
            var path = WebUtility.UrlEncode(folderPath);
            
            return await GetJsonAsync<List<File>>($"{Apiurl}/{siteId}/{path}");
        }

        public async Task<File> GetFileAsync(int fileId)
        {
            return await GetJsonAsync<File>($"{Apiurl}/{fileId.ToString()}");
        }

        public async Task<File> AddFileAsync(File file)
        {
            return await PostJsonAsync<File>(Apiurl, file);
        }

        public async Task<File> UpdateFileAsync(File file)
        {
            return await PutJsonAsync<File>($"{Apiurl}/{file.FileId.ToString()}", file);
        }

        public async Task DeleteFileAsync(int fileId)
        {
            await DeleteAsync($"{Apiurl}/{fileId.ToString()}");
        }

        public async Task<File> UploadFileAsync(string url, int folderId)
        {
            return await GetJsonAsync<File>($"{Apiurl}/upload?url={WebUtility.UrlEncode(url)}&folderid={folderId.ToString()}");
        }

        public async Task<string> UploadFilesAsync(int folderId, string[] files, string id)
        {
            return await UploadFilesAsync(folderId.ToString(), files, id);
        }

        public async Task<string> UploadFilesAsync(string folder, string[] files, string id)
        {
            string result = "";

            var interop = new Interop(_jsRuntime);
            await interop.UploadFiles($"{Apiurl}/upload", folder, id);

            // uploading files is asynchronous so we need to wait for the upload to complete
            bool success = false;
            int attempts = 0;
            while (attempts < 5 && success == false)
            {
                Thread.Sleep(2000); // wait 2 seconds
                result = "";

                List<File> fileList = await GetFilesAsync(folder);
                if (fileList.Count > 0)
                {
                    success = true;
                    foreach (string file in files)
                    {
                        if (!fileList.Exists(item => item.Name == file))
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

        public async Task<byte[]> DownloadFileAsync(int fileId)
        {
            return await GetByteArrayAsync($"{Apiurl}/download/{fileId.ToString()}");
        }
    }
}
