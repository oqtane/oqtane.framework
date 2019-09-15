using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IFileService
    {
        Task UploadFilesAsync(string Folder);
        Task UploadFilesAsync(string Folder, string FileUploadName);
    }
}
