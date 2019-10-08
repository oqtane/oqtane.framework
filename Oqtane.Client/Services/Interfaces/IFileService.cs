using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IFileService
    {
        Task<List<string>> GetFilesAsync(string Folder);
        Task<bool> UploadFilesAsync(string Folder, string[] Files, string FileUploadName);
    }
}
