using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class FileRepository : IFileRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;

        public FileRepository(TenantDBContext context, IPermissionRepository permissions)
        {
            _db = context;
            _permissions = permissions;
        }

        public IEnumerable<File> GetFiles(int folderId)
        {
            IEnumerable<Permission> permissions = _permissions.GetPermissions("Folder", folderId).ToList();
            IEnumerable<File> files = _db.File.Where(item => item.FolderId == folderId).Include(item => item.Folder);
            foreach (File file in files)
            {
                file.Folder.Permissions = _permissions.EncodePermissions(folderId, permissions);
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
            File file = _db.File.Where(item => item.FileId == fileId).Include(item => item.Folder).FirstOrDefault();
            if (file != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Folder", file.FolderId).ToList();
                file.Folder.Permissions = _permissions.EncodePermissions(file.FolderId, permissions);
            }
            return file;
        }

        public void DeleteFile(int fileId)
        {
            File file = _db.File.Find(fileId);
            _db.File.Remove(file);
            _db.SaveChanges();
        }
    }
}
