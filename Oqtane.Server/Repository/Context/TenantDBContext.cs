using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Repository.Databases.Interfaces;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Repository
{
    public class TenantDBContext : DBContextBase, IMultiDatabase
    {
        public TenantDBContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies) { }

        public virtual DbSet<Site> Site { get; set; }
        public virtual DbSet<Page> Page { get; set; }
        public virtual DbSet<PageModule> PageModule { get; set; }
        public virtual DbSet<Module> Module { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }
        public virtual DbSet<Permission> Permission { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<Folder> Folder { get; set; }
        public virtual DbSet<File> File { get; set; }
        public virtual DbSet<Language> Language { get; set; }
        public virtual DbSet<Visitor> Visitor { get; set; }
        public virtual DbSet<UrlMapping> UrlMapping { get; set; }
    }
}
