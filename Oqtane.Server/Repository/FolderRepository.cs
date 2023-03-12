using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class FolderRepository : IFolderRepository
    {
        private TenantDBContext _db;
        private readonly IPermissionRepository _permissions;
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantManager _tenants;

        public FolderRepository(TenantDBContext context, IPermissionRepository permissions,IWebHostEnvironment environment, ITenantManager tenants)
        {
            _db = context;
            _permissions = permissions;
            _environment = environment;
            _tenants = tenants;
        }

        public IEnumerable<Folder> GetFolders(int siteId)
        {
            IEnumerable<Permission> permissions = _permissions.GetPermissions(siteId, EntityNames.Folder).ToList();
            IEnumerable<Folder> folders = _db.Folder.Where(item => item.SiteId == siteId);
            foreach(Folder folder in folders)
            {
                folder.PermissionList = permissions.Where(item => item.EntityId == folder.FolderId).ToList();
            }
            return folders;
        }

        public Folder AddFolder(Folder folder)
        {
            folder.IsDeleted = false;
            _db.Folder.Add(folder);
            _db.SaveChanges();
            _permissions.UpdatePermissions(folder.SiteId, EntityNames.Folder, folder.FolderId, folder.PermissionList);
            return folder;
        }

        public Folder UpdateFolder(Folder folder)
        {
            _db.Entry(folder).State = EntityState.Modified;
            _db.SaveChanges();
            _permissions.UpdatePermissions(folder.SiteId, EntityNames.Folder, folder.FolderId, folder.PermissionList);
            return folder;
        }

        public Folder GetFolder(int folderId)
        {
            return GetFolder(folderId, true);
        }

        public Folder GetFolder(int folderId, bool tracking)
        {
            Folder folder;
            if (tracking)
            {
                folder = _db.Folder.Find(folderId);
            }
            else
            {
                folder = _db.Folder.AsNoTracking().Where(item => item.FolderId == folderId).FirstOrDefault();
            }
            if (folder != null)
            {
                folder.PermissionList = _permissions.GetPermissions(folder.SiteId, EntityNames.Folder, folder.FolderId)?.ToList();
            }
            return folder;
        }

        public Folder GetFolder(int siteId, string path)
        {
            Folder folder = _db.Folder.Where(item => item.SiteId == siteId && item.Path == path).FirstOrDefault();
            if (folder != null)
            {
                folder.PermissionList = _permissions.GetPermissions(folder.SiteId, EntityNames.Folder, folder.FolderId)?.ToList();
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

        public string GetFolderPath(int folderId)
        {
            Folder folder = _db.Folder.Find(folderId);
            return GetFolderPath(folder);
        }

        public string GetFolderPath(Folder folder)
        {
            string path = "";
            switch (folder.Type)
            {
                case FolderTypes.Private:
                    path = Utilities.PathCombine(_environment.ContentRootPath, "Content", "Tenants", _tenants.GetTenant().TenantId.ToString(), "Sites", folder.SiteId.ToString(), folder.Path);
                    break;
                case FolderTypes.Public:
                    path = Utilities.PathCombine(_environment.WebRootPath, "Content", "Tenants", _tenants.GetTenant().TenantId.ToString(), "Sites", folder.SiteId.ToString(), folder.Path);
                    break;
            }
            return path;
        }

    }
}
