using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;
using File = Oqtane.Models.File;

namespace Oqtane.Repository
{
    public interface IFileRepository
    {
        IEnumerable<File> GetFiles(int folderId);
        IEnumerable<File> GetFiles(int folderId, bool tracking);
        File AddFile(File file);
        File UpdateFile(File file);
        File GetFile(int fileId);
        File GetFile(int fileId, bool tracking);
        File GetFile(int folderId, string fileName);
        File GetFile(int siteId, string folderPath, string fileName);
        void DeleteFile(int fileId);
        string GetFilePath(int fileId);
        string GetFilePath(File file);
    }

    public class FileRepository : IFileRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IPermissionRepository _permissions;
        private readonly IFolderRepository _folderRepository;
        private readonly ITenantManager _tenants;

        public FileRepository(IDbContextFactory<TenantDBContext> dbContextFactory, IPermissionRepository permissions, IFolderRepository folderRepository, ITenantManager tenants)
        {
            _dbContextFactory = dbContextFactory;
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
            using var db = _dbContextFactory.CreateDbContext();
            var folder = _folderRepository.GetFolder(folderId, false);
            var permissions = _permissions.GetPermissions(folder.SiteId, EntityNames.Folder, folderId).ToList();

            IEnumerable<File> files;
            if (tracking)
            {
                files = db.File.Where(item => item.FolderId == folderId).Include(item => item.Folder).ToList();
            }
            else
            {
                files = db.File.AsNoTracking().Where(item => item.FolderId == folderId).Include(item => item.Folder).ToList();
            }

            var alias = _tenants.GetAlias();
            foreach (var file in files)
            {
                file.Folder.PermissionList = permissions.ToList();
                file.Url = GetFileUrl(file, alias);
            }
            return files;
        }

        public File AddFile(File file)
        {
            using var db = _dbContextFactory.CreateDbContext();
            file.IsDeleted = false;
            db.File.Add(file);
            db.SaveChanges();
            file.Folder = _folderRepository.GetFolder(file.FolderId);
            file.Url = GetFileUrl(file, _tenants.GetAlias());
            return file;
        }

        public File UpdateFile(File file)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(file).State = EntityState.Modified;
            db.SaveChanges();
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
            using var db = _dbContextFactory.CreateDbContext();
            File file;
            if (tracking)
            {
                file = db.File.Include(item => item.Folder).FirstOrDefault(item => item.FileId == fileId);
            }
            else
            {
                file = db.File.AsNoTracking().Include(item => item.Folder).FirstOrDefault(item => item.FileId == fileId);
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
            using var db = _dbContextFactory.CreateDbContext();
            var file = db.File.AsNoTracking()
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
            using var db = _dbContextFactory.CreateDbContext();
            var file = db.File.AsNoTracking()
                .Include(item => item.Folder)
                .FirstOrDefault(item => item.Folder.SiteId == siteId &&
                    item.Folder.Path.ToLower() == folderPath &&
                    item.Name.ToLower() == fileName);

            if (file != null)
            {
                var permissions = _permissions.GetPermissions(file.Folder.SiteId, EntityNames.Folder, file.FolderId).ToList();
                file.Folder.PermissionList = permissions.ToList();
                file.Url = GetFileUrl(file, _tenants.GetAlias());
            }

            return file;
        }

        public void DeleteFile(int fileId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var file = db.File.Find(fileId);
            db.File.Remove(file);
            db.SaveChanges();
        }

        public string GetFilePath(int fileId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var file = db.File.Find(fileId);
            return GetFilePath(file);
        }

        public string GetFilePath(File file)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (file == null) return null;
            var folder = file.Folder ?? db.Folder.AsNoTracking().FirstOrDefault(item => item.FolderId == file.FolderId);
            var filepath = Path.Combine(_folderRepository.GetFolderPath(folder), file.Name);
            return filepath;
        }

        private string GetFileUrl(File file, Alias alias)
        {
            return Utilities.FileUrl(alias, file.Folder.Path, file.Name);
        }
    }
}
