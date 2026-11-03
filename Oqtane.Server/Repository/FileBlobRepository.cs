using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IFileBlobRepository
    {
        FileBlob GetFileBlob(int folderId, string fileName);
        FileBlob AddFileBlob(FileBlob blob);
        FileBlob UpdateFileBlob(FileBlob blob);
        void DeleteFileBlob(int folderId, string fileName);
    }

    public class FileBlobRepository : IFileBlobRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly ITenantManager _tenants;

        public FileBlobRepository(IDbContextFactory<TenantDBContext> dbContextFactory, ITenantManager tenants)
        {
            _dbContextFactory = dbContextFactory;
            _tenants = tenants;
        }

        public FileBlob GetFileBlob(int folderId, string fileName)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.FileBlob.AsNoTracking().FirstOrDefault(i => i.FolderId == folderId && i.FileName.Equals(fileName));
        }

        public FileBlob AddFileBlob(FileBlob blob)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.FileBlob.Add(blob);
            db.SaveChanges();

            return blob;
        }
        public FileBlob UpdateFileBlob(FileBlob blob)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(blob).State = EntityState.Modified;
            db.SaveChanges();

            return blob;
        }
        public void DeleteFileBlob(int folderId, string fileName)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var blob = GetFileBlob(folderId, fileName);
            if(blob != null)
            {
                db.FileBlob.Remove(blob);
                db.SaveChanges();
            }
        }
    }
}
