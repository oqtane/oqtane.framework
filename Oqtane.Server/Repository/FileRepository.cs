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
            IEnumerable<Permission> permissions = _permissions.GetPermissions(EntityNames.Folder, folderId).ToList();
            IEnumerable<File> files = _db.File.Where(item => item.FolderId == folderId).Include(item => item.Folder);
            var alias = _tenants.GetAlias();
            foreach (File file in files)
            {
                file.Folder.Permissions = permissions.EncodePermissions();
                file.Url = GetFileUrl(file, alias);
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
                IEnumerable<Permission> permissions = _permissions.GetPermissions(EntityNames.Folder, file.FolderId).ToList();
                file.Folder.Permissions = permissions.EncodePermissions();
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
            var folder = file.Folder ?? _db.Folder.Find(file.FolderId);
            var filepath = Path.Combine(_folderRepository.GetFolderPath(folder), file.Name);
            return filepath;
        }

        private string GetFileUrl(File file, Alias alias)
        {
            string url = "";
            switch (file.Folder.Type)
            {
                case FolderTypes.Private:
                    url = Utilities.ContentUrl(alias, file.FileId);
                    break;
                case FolderTypes.Public:
                    url = "/" + Utilities.UrlCombine("Content", "Tenants", alias.TenantId.ToString(), "Sites", file.Folder.SiteId.ToString(), file.Folder.Path) + file.Name;
                    break;
            }
            return url;
        }
    }
}
