using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IFileService
    {
        Task<List<File>> GetFilesAsync(int folderId);
        Task<List<File>> GetFilesAsync(string folder);
        Task<File> GetFileAsync(int fileId);
        Task<File> AddFileAsync(File file);
        Task<File> UpdateFileAsync(File file);
        Task DeleteFileAsync(int fileId);
        Task<File> UploadFileAsync(string url, int folderId);
        Task<string> UploadFilesAsync(int folderId, string[] files, string fileUploadName);
        Task<string> UploadFilesAsync(string folder, string[] files, string fileUploadName);
        Task<byte[]> DownloadFileAsync(int fileId);

        Task<List<File>> GetFilesAsync(int siteId, string folderPath);
    }
}
