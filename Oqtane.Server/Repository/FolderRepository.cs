using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class FolderRepository : IFolderRepository
    {
        private TenantDBContext db;
        private readonly IPermissionRepository Permissions;

        public FolderRepository(TenantDBContext context, IPermissionRepository Permissions)
        {
            db = context;
            this.Permissions = Permissions;
        }

        public IEnumerable<Folder> GetFolders(int SiteId)
        {
            IEnumerable<Permission> permissions = Permissions.GetPermissions(SiteId, "Folder").ToList();
            IEnumerable<Folder> folders = db.Folder.Where(item => item.SiteId == SiteId);
            foreach(Folder folder in folders)
            {
                folder.Permissions = Permissions.EncodePermissions(folder.FolderId, permissions);
            }
            return folders;
        }

        public Folder AddFolder(Folder Folder)
        {
            db.Folder.Add(Folder);
            db.SaveChanges();
            Permissions.UpdatePermissions(Folder.SiteId, "Folder", Folder.FolderId, Folder.Permissions);
            return Folder;
        }

        public Folder UpdateFolder(Folder Folder)
        {
            db.Entry(Folder).State = EntityState.Modified;
            db.SaveChanges();
            Permissions.UpdatePermissions(Folder.SiteId, "Folder", Folder.FolderId, Folder.Permissions);
            return Folder;
        }

        public Folder GetFolder(int FolderId)
        {
            Folder folder = db.Folder.Find(FolderId);
            if (folder != null)
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions("Folder", folder.FolderId).ToList();
                folder.Permissions = Permissions.EncodePermissions(folder.FolderId, permissions);
            }
            return folder;
        }

        public Folder GetFolder(int SiteId, string Path)
        {
            Folder folder = db.Folder.Where(item => item.SiteId == SiteId && item.Path == Path).FirstOrDefault();
            if (folder != null)
            {
                IEnumerable<Permission> permissions = Permissions.GetPermissions("Folder", folder.FolderId).ToList();
                folder.Permissions = Permissions.EncodePermissions(folder.FolderId, permissions);
            }
            return folder;
        }

        public void DeleteFolder(int FolderId)
        {
            Folder Folder = db.Folder.Find(FolderId);
            Permissions.DeletePermissions(Folder.SiteId, "Folder", FolderId);
            db.Folder.Remove(Folder);
            db.SaveChanges();
        }
    }
}
