using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public interface IFolderRepository
    {
        IEnumerable<Folder> GetFolders(int siteId);
        Folder AddFolder(Folder folder);
        Folder UpdateFolder(Folder folder);
        Folder GetFolder(int folderId);
        Folder GetFolder(int folderId, bool tracking);
        Folder GetFolder(int siteId, string path);
        void DeleteFolder(int folderId);
    }

    public class FolderRepository : IFolderRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IPermissionRepository _permissions;
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantManager _tenants;

        public FolderRepository(IDbContextFactory<TenantDBContext> dbContextFactory, IPermissionRepository permissions,IWebHostEnvironment environment, ITenantManager tenants)
        {
            _dbContextFactory = dbContextFactory;
            _permissions = permissions;
            _environment = environment;
            _tenants = tenants;
        }

        public IEnumerable<Folder> GetFolders(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var permissions = _permissions.GetPermissions(siteId, EntityNames.Folder).ToList();
            var folders = db.Folder.Include(i => i.FolderConfig).Where(item => item.SiteId == siteId).ToList();
            foreach (var folder in folders)
            {
                folder.PermissionList = permissions.Where(item => item.EntityId == folder.FolderId).ToList();
            }
            return folders;
        }

        public Folder AddFolder(Folder folder)
        {
            using var db = _dbContextFactory.CreateDbContext();
            //build the mapped path
            folder.MappedPath = BuildMappedPath(folder);
            db.Folder.Add(folder);
            db.SaveChanges();
            _permissions.UpdatePermissions(folder.SiteId, EntityNames.Folder, folder.FolderId, folder.PermissionList);
            return folder;
        }

        public Folder UpdateFolder(Folder folder)
        {
            using var db = _dbContextFactory.CreateDbContext();
            //build the mapped path
            folder.MappedPath = BuildMappedPath(folder);
            db.Entry(folder).State = EntityState.Modified;
            db.SaveChanges();
            _permissions.UpdatePermissions(folder.SiteId, EntityNames.Folder, folder.FolderId, folder.PermissionList);
            return folder;
        }

        public Folder GetFolder(int folderId)
        {
            return GetFolder(folderId, true);
        }

        public Folder GetFolder(int folderId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            Folder folder;
            if (tracking)
            {
                folder = db.Folder.Include(i => i.FolderConfig).FirstOrDefault(item => item.FolderId == folderId);
            }
            else
            {
                folder = db.Folder.AsNoTracking().Include(i => i.FolderConfig).Where(item => item.FolderId == folderId).FirstOrDefault();
            }
            if (folder != null)
            {
                folder.PermissionList = _permissions.GetPermissions(folder.SiteId, EntityNames.Folder, folder.FolderId)?.ToList();
            }
            return folder;
        }

        public Folder GetFolder(int siteId, string path)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var folder = db.Folder.Include(i => i.FolderConfig).Where(item => item.SiteId == siteId && item.Path == path).FirstOrDefault();
            if (folder != null)
            {
                folder.PermissionList = _permissions.GetPermissions(folder.SiteId, EntityNames.Folder, folder.FolderId)?.ToList();
            }
            return folder;
        }

        public void DeleteFolder(int folderId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var folder = db.Folder.Find(folderId);
            _permissions.DeletePermissions(folder.SiteId, EntityNames.Folder, folderId);
            db.Folder.Remove(folder);
            db.SaveChanges();
        }

        private string BuildMappedPath(Folder folder)
        {
            var path = string.Empty;
            if(folder.ParentId != null)
            {
                var parentFolder = GetFolder(folder.ParentId.Value);
                if (parentFolder != null)
                {
                    var folderName = folder.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
                    path = parentFolder.FolderConfigId != folder.FolderConfigId ? string.Empty : $"{parentFolder.MappedPath}{folderName}/";
                }
            }

            return path;
        }
    }
}
