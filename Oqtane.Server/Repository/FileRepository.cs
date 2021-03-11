using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Shared;
using File = Oqtane.Models.File;

namespace Oqtane.Repository
{
    public class FileRepository : IFileRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;
        private readonly IFolderRepository _folderRepository;

        public FileRepository(TenantDBContext context, IPermissionRepository permissions, IFolderRepository folderRepository)
        {
            _db = context;
            _permissions = permissions;
            _folderRepository = folderRepository;
        }

        public IEnumerable<File> GetFiles(int folderId)
        {
            IEnumerable<Permission> permissions = _permissions.GetPermissions(EntityNames.Folder, folderId).ToList();
            IEnumerable<File> files = _db.File.Where(item => item.FolderId == folderId).Include(item => item.Folder);
            foreach (File file in files)
            {
                file.Folder.Permissions = permissions.EncodePermissions();
            }
            return files;
        }

        public File AddFile(File file)
        {
            _db.File.Add(file);
            _db.SaveChanges();
            return file;
        }

        public File UpdateFile(File file)
        {
            _db.Entry(file).State = EntityState.Modified;
            _db.SaveChanges();
            return file;
        }

        public File GetFile(int fileId)
        {
            return GetFile(fileId, true);
        }

        public File GetFile(int fileId, bool tracking)
        {
            File file;
            if (tracking)
            {
                file = _db.File.Where(item => item.FileId == fileId).Include(item => item.Folder).FirstOrDefault();

            }
            else
            {
                file = _db.File.AsNoTracking().Where(item => item.FileId == fileId).Include(item => item.Folder).FirstOrDefault();
            }
            if (file != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions(EntityNames.Folder, file.FolderId).ToList();
                file.Folder.Permissions = permissions.EncodePermissions();
            }
            return file;
        }

        public void DeleteFile(int fileId)
        {
            File file = _db.File.Find(fileId);
            _db.File.Remove(file);
            _db.SaveChanges();
        }

        public string GetFilePath(int fileId)
        {
            var file = _db.File.Find(fileId);
            return GetFilePath(file);
        }

        public string GetFilePath(File file)
        {
            if (file == null) return null;
            var folder = file.Folder ?? _db.Folder.Find(file.FolderId);
            var filepath = Path.Combine(_folderRepository.GetFolderPath(folder), file.Name);
            return filepath;
        }
    }
}
