using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class FolderRepository : IFolderRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;

        public FolderRepository(TenantDBContext context, IPermissionRepository permissions)
        {
            _db = context;
            _permissions = permissions;
        }

        public IEnumerable<Folder> GetFolders(int siteId)
        {
            IEnumerable<Permission> permissions = _permissions.GetPermissions(siteId, EntityNames.Folder).ToList();
            IEnumerable<Folder> folders = _db.Folder.Where(item => item.SiteId == siteId);
            foreach(Folder folder in folders)
            {
                folder.Permissions = permissions.Where(item => item.EntityId == folder.FolderId).EncodePermissions();
            }
            return folders;
        }

        public Folder AddFolder(Folder folder)
        {
            _db.Folder.Add(folder);
            _db.SaveChanges();
            _permissions.UpdatePermissions(folder.SiteId, EntityNames.Folder, folder.FolderId, folder.Permissions);
            return folder;
        }

        public Folder UpdateFolder(Folder folder)
        {
            _db.Entry(folder).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(folder.SiteId, EntityNames.Folder, folder.FolderId, folder.Permissions);
            return folder;
        }

        public Folder GetFolder(int folderId)
        {
            Folder folder = _db.Folder.Find(folderId);
            if (folder != null)
            {
                folder.Permissions = _permissions.GetPermissionString(EntityNames.Folder, folder.FolderId);
            }
            return folder;
        }

        public Folder GetFolder(int siteId, string path)
        {
            Folder folder = _db.Folder.Where(item => item.SiteId == siteId && item.Path == path).FirstOrDefault();
            if (folder != null)
            {
                folder.Permissions = _permissions.GetPermissionString(EntityNames.Folder, folder.FolderId);
            }
            return folder;
        }

        public void DeleteFolder(int folderId)
        {
            Folder folder = _db.Folder.Find(folderId);
            _permissions.DeletePermissions(folder.SiteId, EntityNames.Folder, folderId);
            _db.Folder.Remove(folder);
            _db.SaveChanges();
        }
    }
}
