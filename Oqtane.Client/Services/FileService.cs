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
    /// <summary>
    /// Service to get / create / upload / download files.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Get all <see cref="File"/>s in the specified Folder
        /// </summary>
        /// <param name="folderId">The folder ID</param>
        /// <returns></returns>
        Task<List<File>> GetFilesAsync(int folderId);

        /// <summary>
        /// Get all <see cref="File"/>s in the specified folder. 
        /// </summary>
        /// <param name="folder">
        /// The folder path relative to where the files are stored.
        /// TODO: todoc verify exactly from where the folder path must start
        /// </param>
        /// <returns></returns>
        Task<List<File>> GetFilesAsync(string folder);

        /// <summary>
        /// Get one <see cref="File"/>
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<File> GetFileAsync(int fileId);

        /// <summary>
        /// Get a <see cref="File"/> based on the <see cref="Folder"/> and file name.
        /// </summary>
        /// <param name="folderId">Reference to the <see cref="Folder"/></param>
        /// <param name="name">name of the file
        /// </param>
        /// <returns></returns>
        Task<File> GetFileAsync(int folderId, string name);

        /// <summary>
        /// Add / store a <see cref="File"/> record.
        /// This does not contain the file contents. 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<File> AddFileAsync(File file);

        /// <summary>
        /// Update a <see cref="File"/> record.
        /// Use this for rename a file or change some attributes. 
        /// This does not contain the file contents. 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<File> UpdateFileAsync(File file);

        /// <summary>
        /// Delete a <see cref="File"/>
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task DeleteFileAsync(int fileId);

        /// <summary>
        /// Upload a file from a URL to a <see cref="Folder"/>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="folderId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<File> UploadFileAsync(string url, int folderId, string name);

        /// <summary>
        /// Get / download a file (the body).
        /// </summary>
        /// <param name="fileId">Reference to a <see cref="File"/></param>
        /// <returns>The bytes of the file</returns>
        Task<byte[]> DownloadFileAsync(int fileId);

        /// <summary>
        /// Retrieve a list of files from a <see cref="Site"/> and <see cref="Folder"/>
        /// </summary>
        /// <param name="siteId">Reference to the <see cref="Site"/></param>
        /// <param name="folderPath">Path of the folder
        /// TODO: todoc verify exactly from where the folder path must start
        /// </param>
        /// <returns></returns>
        Task<List<File>> GetFilesAsync(int siteId, string folderPath);

        /// <summary>
        /// Unzips the contents of a zip file
        /// </summary>
        /// <param name="fileId">Reference to the <see cref="File"/></param>
        /// <returns></returns>
        Task UnzipFileAsync(int fileId);
    }

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

        public async Task UnzipFileAsync(int fileId)
        {
            await PutAsync($"{Apiurl}/unzip/{fileId}");
        }
    }
}
