using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class FileRepository : IFileRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;

        public FileRepository(TenantDBContext context, IPermissionRepository Permissions)
        {
            _db = context;
            _permissions = Permissions;
        }

        public IEnumerable<File> GetFiles(int FolderId)
        {
            IEnumerable<Permission> permissions = _permissions.GetPermissions("Folder", FolderId).ToList();
            IEnumerable<File> files = _db.File.Where(item => item.FolderId == FolderId).Include(item => item.Folder);
            foreach (File file in files)
            {
                file.Folder.Permissions = _permissions.EncodePermissions(FolderId, permissions);
            }
            return files;
        }

        public File AddFile(File File)
        {
            _db.File.Add(File);
            _db.SaveChanges();
            return File;
        }

        public File UpdateFile(File File)
        {
            _db.Entry(File).State = EntityState.Modified;
            _db.SaveChanges();
            return File;
        }

        public File GetFile(int FileId)
        {
            File file = _db.File.Where(item => item.FileId == FileId).Include(item => item.Folder).FirstOrDefault();
            if (file != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Folder", file.FolderId).ToList();
                file.Folder.Permissions = _permissions.EncodePermissions(file.FolderId, permissions);
            }
            return file;
        }

        public void DeleteFile(int FileId)
        {
            File File = _db.File.Find(FileId);
            _db.File.Remove(File);
            _db.SaveChanges();
        }
    }
}
