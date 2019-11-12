using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IFileService
    {
        Task<List<string>> GetFilesAsync(string Folder);
        Task<string> UploadFilesAsync(string Folder, string[] Files, string FileUploadName);
        Task DeleteFileAsync(string Folder, string File);
    }
}
