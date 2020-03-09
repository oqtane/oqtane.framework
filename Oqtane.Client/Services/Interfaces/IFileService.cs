using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IFileService
    {
        Task<List<File>> GetFilesAsync(int FolderId);
        Task<List<File>> GetFilesAsync(string Folder);
        Task<File> GetFileAsync(int FileId);
        Task<File> AddFileAsync(File File);
        Task<File> UpdateFileAsync(File File);
        Task DeleteFileAsync(int FileId);
        Task<File> UploadFileAsync(string Url, int FolderId);
        Task<string> UploadFilesAsync(int FolderId, string[] Files, string FileUploadName);
        Task<string> UploadFilesAsync(string Folder, string[] Files, string FileUploadName);
        Task<byte[]> DownloadFileAsync(int FileId);

        Task<List<File>> GetFilesAsync(int siteId, string folderPath);
    }
}
