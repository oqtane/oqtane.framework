using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
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
        private readonly ITenantManager _tenants;

        public FileRepository(TenantDBContext context, IPermissionRepository permissions, IFolderRepository folderRepository, ITenantManager tenants)
        {
            _db = context;
            _permissions = permissions;
            _folderRepository = folderRepository;
            _tenants = tenants;
        }

        public IEnumerable<File> GetFiles(int folderId)
        {
            return GetFiles(folderId, true);
        }

        public IEnumerable<File> GetFiles(int folderId, bool tracking)
        {
            var alias = _tenants.GetAlias();
            IEnumerable<Permission> permissions = _permissions.GetPermissions(alias.SiteId, EntityNames.Folder, folderId).ToList();
            IEnumerable<File> files;
            if (tracking)
            {
                files = _db.File.Where(item => item.FolderId == folderId).Include(item => item.Folder);
            }
            else
            {
                files = _db.File.AsNoTracking().Where(item => item.FolderId == folderId).Include(item => item.Folder);
            }
            foreach (File file in files)
            {
                file.Folder.PermissionList = permissions.ToList();
                file.Url = GetFileUrl(file, alias);
            }
            return files;
        }

        public File AddFile(File file)
        {
            file.IsDeleted = false;
            _db.File.Add(file);
            _db.SaveChanges();
            file.Folder = _folderRepository.GetFolder(file.FolderId);
            file.Url = GetFileUrl(file, _tenants.GetAlias());
            return file;
        }

        public File UpdateFile(File file)
        {
            _db.Entry(file).State = EntityState.Modified;
            _db.SaveChanges();
            file.Folder = _folderRepository.GetFolder(file.FolderId);
            file.Url = GetFileUrl(file, _tenants.GetAlias());
            return file;
        }

        public File GetFile(int fileId)
        {
            File file = GetFile(fileId, true);
            return file;
        }

        public File GetFile(int fileId, bool tracking)
        {
            File file;
            if (tracking)
            {
                file = _db.File.Include(item => item.Folder).FirstOrDefault(item => item.FileId == fileId);
            }
            else
            {
                file = _db.File.AsNoTracking().Include(item => item.Folder).FirstOrDefault(item => item.FileId == fileId);
            }
            if (file != null)
            {
                file.Folder.PermissionList = _permissions.GetPermissions(file.Folder.SiteId, EntityNames.Folder, file.FolderId).ToList();
                file.Url = GetFileUrl(file, _tenants.GetAlias());
            }
            return file;
        }

        public File GetFile(int folderId, string fileName)
        {
            var file = _db.File.AsNoTracking()
                .Include(item => item.Folder)
                .FirstOrDefault(item => item.FolderId == folderId &&
                    item.Name.ToLower() == fileName);

            if (file != null)
            {
                file.Folder.PermissionList = _permissions.GetPermissions(file.Folder.SiteId, EntityNames.Folder, file.FolderId).ToList();
                file.Url = GetFileUrl(file, _tenants.GetAlias());
            }

            return file;
        }

        public File GetFile(int siteId, string folderPath, string fileName)
        {
            var file = _db.File.AsNoTracking()
                .Include(item => item.Folder)
                .FirstOrDefault(item => item.Folder.SiteId == siteId &&
                    item.Folder.Path.ToLower() == folderPath &&
                    item.Name.ToLower() == fileName);

            if (file != null)
            {
                IEnumerable<Permission> permissions = _permissions.GetPermissions(file.Folder.SiteId, EntityNames.Folder, file.FolderId).ToList();
                file.Folder.PermissionList = permissions.ToList();
                file.Url = GetFileUrl(file, _tenants.GetAlias());
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
            var folder = file.Folder ?? _db.Folder.AsNoTracking().FirstOrDefault(item => item.FolderId == file.FolderId);
            var filepath = Path.Combine(_folderRepository.GetFolderPath(folder), file.Name);
            return filepath;
        }

        private string GetFileUrl(File file, Alias alias)
        {
            return Utilities.FileUrl(alias, file.Folder.Path, file.Name);
        }
    }
}
