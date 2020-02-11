using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class FileRepository : IFileRepository
    {
        private TenantDBContext db;
        private readonly IPermissionRepository Permissions;

        public FileRepository(TenantDBContext context, IPermissionRepository Permissions)
        {
            db = context;
            this.Permissions = Permissions;
        }

        public IEnumerable<File> GetFiles(int FolderId)
        {
            IEnumerable<Permission> permissions = Permissions.GetPermissions("Folder", FolderId).ToList();
            IEnumerable<File> files = db.File.Where(item => item.FolderId == FolderId).Include(item => item.Folder);
            foreach (File file in files)
            {
                file.Folder.Permissions = Permissions.EncodePermissions(FolderId, permissions);
            }
            return files;
        }

        public File AddFile(File File)
        {
            db.File.Add(File);
            db.SaveChanges();
            return File;
        }

        public File UpdateFile(File File)
        {
            db.Entry(File).State = EntityState.Modified;
            db.SaveChanges();
            return File;
        }

        public File GetFile(int FileId)
        {
            File file = db.File.Where(item => item.FileId == FileId).Include(item => item.Folder).FirstOrDefault();
            if (file != null)
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions("Folder", file.FolderId).ToList();
                file.Folder.Permissions = Permissions.EncodePermissions(file.FolderId, permissions);
            }
            return file;
        }

        public void DeleteFile(int FileId)
        {
            File File = db.File.Find(FileId);
            db.File.Remove(File);
            db.SaveChanges();
        }
    }
}
