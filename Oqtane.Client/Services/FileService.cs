using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class FileService : ServiceBase, IFileService
    {
        public FileService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("File");

        public async Task<List<File>> GetFilesAsync(int folderId)
        {
            return await GetFilesAsync(folderId.ToString());
        }

        public async Task<List<File>> GetFilesAsync(string folder)
        {
            List<File> files = await GetJsonAsync<List<File>>($"{Apiurl}?folder={folder}");
            return files.OrderBy(item => item.Name).ToList();
        }

        public async Task<List<File>> GetFilesAsync(int siteId, string folderPath)
        {
            if (!(string.IsNullOrEmpty(folderPath) || folderPath.EndsWith(System.IO.Path.DirectorySeparatorChar) || folderPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar)))
            {
                folderPath = Utilities.UrlCombine(folderPath) + "/";
            }

            var path = WebUtility.UrlEncode(folderPath);

            List<File> files = await GetJsonAsync<List<File>>($"{Apiurl}/{siteId}/{path}");
            return files?.OrderBy(item => item.Name).ToList();
        }

        public async Task<File> GetFileAsync(int fileId)
        {
            return await GetJsonAsync<File>($"{Apiurl}/{fileId}");
        }

        public async Task<File> GetFileAsync(int folderId, string name)
        {
            return await GetJsonAsync<File>($"{Apiurl}/name/{name}/{folderId}");
        }

        public async Task<File> AddFileAsync(File file)
        {
            return await PostJsonAsync<File>(Apiurl, file);
        }

        public async Task<File> UpdateFileAsync(File file)
        {
            return await PutJsonAsync<File>($"{Apiurl}/{file.FileId}", file);
        }

        public async Task DeleteFileAsync(int fileId)
        {
            await DeleteAsync($"{Apiurl}/{fileId}");
        }

        public async Task<File> UploadFileAsync(string url, int folderId, string name)
        {
            return await GetJsonAsync<File>($"{Apiurl}/upload?url={WebUtility.UrlEncode(url)}&folderid={folderId}&name={name}");
        }

        public async Task<byte[]> DownloadFileAsync(int fileId)
        {
            return await GetByteArrayAsync($"{Apiurl}/download/{fileId}");
        }
    }
}
