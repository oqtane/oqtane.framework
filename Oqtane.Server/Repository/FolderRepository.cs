using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class FolderRepository : IFolderRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;

        public FolderRepository(TenantDBContext context, IPermissionRepository Permissions)
        {
            _db = context;
            _permissions = Permissions;
        }

        public IEnumerable<Folder> GetFolders(int SiteId)
        {
            IEnumerable<Permission> permissions = _permissions.GetPermissions(SiteId, "Folder").ToList();
            IEnumerable<Folder> folders = _db.Folder.Where(item => item.SiteId == SiteId);
            foreach(Folder folder in folders)
            {
                folder.Permissions = _permissions.EncodePermissions(folder.FolderId, permissions);
            }
            return folders;
        }

        public Folder AddFolder(Folder Folder)
        {
            _db.Folder.Add(Folder);
            _db.SaveChanges();
            _permissions.UpdatePermissions(Folder.SiteId, "Folder", Folder.FolderId, Folder.Permissions);
            return Folder;
        }

        public Folder UpdateFolder(Folder Folder)
        {
            _db.Entry(Folder).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(Folder.SiteId, "Folder", Folder.FolderId, Folder.Permissions);
            return Folder;
        }

        public Folder GetFolder(int FolderId)
        {
            Folder folder = _db.Folder.Find(FolderId);
            if (folder != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Folder", folder.FolderId).ToList();
                folder.Permissions = _permissions.EncodePermissions(folder.FolderId, permissions);
            }
            return folder;
        }

        public Folder GetFolder(int SiteId, string Path)
        {
            Folder folder = _db.Folder.Where(item => item.SiteId == SiteId && item.Path == Path).FirstOrDefault();
            if (folder != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions("Folder", folder.FolderId).ToList();
                folder.Permissions = _permissions.EncodePermissions(folder.FolderId, permissions);
            }
            return folder;
        }

        public void DeleteFolder(int FolderId)
        {
            Folder Folder = _db.Folder.Find(FolderId);
            _permissions.DeletePermissions(Folder.SiteId, "Folder", FolderId);
            _db.Folder.Remove(Folder);
            _db.SaveChanges();
        }
    }
}
