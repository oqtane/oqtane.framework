using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Interfaces;
using File = System.IO.File;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
// ReSharper disable UseIndexFromEndExpression

namespace Oqtane.Infrastructure
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly IConfigurationRoot _config;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMemoryCache _cache;

        public DatabaseManager(IConfigurationRoot config, IServiceScopeFactory serviceScopeFactory, IMemoryCache cache)
        {
            _config = config;
            _serviceScopeFactory = serviceScopeFactory;
            _cache = cache;
        }

        public Installation IsInstalled()
        {
            var result = new Installation { Success = false, Message = string.Empty };
            if (!string.IsNullOrEmpty(_config.GetConnectionString(SettingKeys.ConnectionStringKey)))
            {
                result.Success = true;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<MasterDBContext>();
                    if (db.Database.CanConnect())
                    {
                        try
                        {
                            var provisioned = db.Tenant.Any();
                        }
                        catch
                        {
                            result.Message = "Master Database Not Installed Correctly";
                        }
                    }
                    else
                    {
                        result.Message = "Cannot Connect To Master Database";
                    }
                }
            }
            return result;
        }

        public Installation Install()
        {
            return Install(null);
        }

        public Installation Install(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            // get configuration
            if (install == null)
            {
                // startup or silent installation
                install = new InstallConfig
                {
                    ConnectionString = _config.GetConnectionString(SettingKeys.ConnectionStringKey),
                    TenantName = TenantNames.Master,
                    DatabaseType = _config.GetSection(SettingKeys.DatabaseSection)[SettingKeys.DatabaseTypeKey],
                    IsNewTenant = false
                };

                var installation = IsInstalled();
                if (!installation.Success)
                {
                    install.Aliases = GetInstallationConfig(SettingKeys.DefaultAliasKey, string.Empty);
                    install.HostPassword = GetInstallationConfig(SettingKeys.HostPasswordKey, string.Empty);
                    install.HostEmail = GetInstallationConfig(SettingKeys.HostEmailKey, string.Empty);

                    if (!string.IsNullOrEmpty(install.ConnectionString) && !string.IsNullOrEmpty(install.Aliases) && !string.IsNullOrEmpty(install.HostPassword) && !string.IsNullOrEmpty(install.HostEmail))
                    {
                        // silent install
                        install.HostName = UserNames.Host;
                        install.SiteTemplate = GetInstallationConfig(SettingKeys.SiteTemplateKey, Constants.DefaultSiteTemplate);
                        install.DefaultTheme = GetInstallationConfig(SettingKeys.DefaultThemeKey, Constants.DefaultTheme);
                        install.DefaultLayout = GetInstallationConfig(SettingKeys.DefaultLayoutKey, Constants.DefaultLayout);
                        install.DefaultContainer = GetInstallationConfig(SettingKeys.DefaultContainerKey, Constants.DefaultContainer);
                        install.SiteName = Constants.DefaultSite;
                        install.IsNewTenant = true;
                    }
                    else
                    {
                        // silent installation is missing required information
                        install.ConnectionString = "";
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(installation.Message))
                    {
                        // problem with prior installation
                        install.ConnectionString = "";
                    }
                }
            }
            else
            {
                // install wizard or add new site
                if (!string.IsNullOrEmpty(install.Aliases))
                {
                    if (string.IsNullOrEmpty(install.SiteTemplate))
                    {
                        install.SiteTemplate = GetInstallationConfig(SettingKeys.SiteTemplateKey, Constants.DefaultSiteTemplate);
                    }
                    if (string.IsNullOrEmpty(install.DefaultTheme))
                    {
                        install.DefaultTheme = GetInstallationConfig(SettingKeys.DefaultThemeKey, Constants.DefaultTheme);
                        if (string.IsNullOrEmpty(install.DefaultLayout))
                        {
                            install.DefaultLayout = GetInstallationConfig(SettingKeys.DefaultLayoutKey, Constants.DefaultLayout);
                        }
                    }
                    if (string.IsNullOrEmpty(install.DefaultContainer))
                    {
                        install.DefaultContainer = GetInstallationConfig(SettingKeys.DefaultContainerKey, Constants.DefaultContainer);
                    }
                }
                else
                {
                    result.Message = "Invalid Installation Configuration";
                    install.ConnectionString = "";
                }
            }

            // proceed with installation/migration
            if (!string.IsNullOrEmpty(install.ConnectionString))
            {
                result = CreateDatabase(install);
                if (result.Success)
                {
                    result = MigrateMaster(install);
                    if (result.Success)
                    {
                        result = CreateTenant(install);
                        if (result.Success)
                        {
                            result = MigrateTenants(install);
                            if (result.Success)
                            {
                                result = MigrateModules(install);
                                if (result.Success)
                                {
                                    result = CreateSite(install);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private Installation CreateDatabase(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            if (install.IsNewTenant)
            {
                try
                {
                    //create data directory if does not exist
                    var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
                    if (!Directory.Exists(dataDirectory)) Directory.CreateDirectory(dataDirectory ?? String.Empty);

                    var connectionString = NormalizeConnectionString(install.ConnectionString);
                    var databaseType = install.DatabaseType;
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var databases = scope.ServiceProvider.GetServices<IOqtaneDatabase>();

                        using (var dbc = new DbContext(new DbContextOptionsBuilder().UseOqtaneDatabase(databases.Single(d => d.Name == databaseType), connectionString).Options))
                        {
                            // create empty database if it does not exist
                            dbc.Database.EnsureCreated();
                            result.Success = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                }
            }
            else
            {
                result.Success = true;
            }

            return result;
        }

        private Installation MigrateMaster(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            if (install.TenantName == TenantNames.Master)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var databases = scope.ServiceProvider.GetServices<IOqtaneDatabase>();
                    var sql = scope.ServiceProvider.GetRequiredService<ISqlRepository>();

                    try
                    {
                        var dbConfig = new DbConfig(null, null, databases) {ConnectionString = install.ConnectionString, DatabaseType = install.DatabaseType};

                        using (var masterDbContext = new MasterDBContext(new DbContextOptions<MasterDBContext>(), dbConfig))
                        {
                            var installation = IsInstalled();
                            if (installation.Success && (install.DatabaseType == "SqlServer" || install.DatabaseType == "LocalDB"))
                            {
                                UpgradeSqlServer(sql, install.ConnectionString, true);
                            }
                            // Push latest model into database
                            masterDbContext.Database.Migrate();
                            result.Success = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = ex.Message;
                    }

                    if (result.Success)
                    {
                        UpdateConnectionString(install.ConnectionString);
                        UpdateDatabaseType(install.DatabaseType);
                    }
                }
            }
            else
            {
                result.Success = true;
            }

            return result;
        }

        private Installation CreateTenant(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            if (!string.IsNullOrEmpty(install.TenantName) && !string.IsNullOrEmpty(install.Aliases))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var databases = scope.ServiceProvider.GetServices<IOqtaneDatabase>();

                    using (var db = GetInstallationContext(databases))
                    {
                        Tenant tenant;
                        if (install.IsNewTenant)
                        {
                            tenant = new Tenant
                            {
                                Name = install.TenantName,
                                DBConnectionString = DenormalizeConnectionString(install.ConnectionString),
                                DBType = install.DatabaseType,
                                CreatedBy = "",
                                CreatedOn = DateTime.UtcNow,
                                ModifiedBy = "",
                                ModifiedOn = DateTime.UtcNow
                            };
                            db.Tenant.Add(tenant);
                            db.SaveChanges();
                            _cache.Remove("tenants");
                        }
                        else
                        {
                            tenant = db.Tenant.FirstOrDefault(item => item.Name == install.TenantName);
                        }

                        foreach (var aliasName in install.Aliases.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (tenant != null)
                            {
                                var alias = new Alias
                                {
                                    Name = aliasName,
                                    TenantId = tenant.TenantId,
                                    SiteId = -1,
                                    CreatedBy = "",
                                    CreatedOn = DateTime.UtcNow,
                                    ModifiedBy = "",
                                    ModifiedOn = DateTime.UtcNow
                                };
                                db.Alias.Add(alias);
                            }

                            db.SaveChanges();
                        }

                        _cache.Remove("aliases");
                    }
                }
            }

            result.Success = true;

            return result;
        }

        private Installation MigrateTenants(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            var versions = Constants.ReleaseVersions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var upgrades = scope.ServiceProvider.GetRequiredService<IUpgradeManager>();
                var databases = scope.ServiceProvider.GetServices<IOqtaneDatabase>();
                var sql = scope.ServiceProvider.GetRequiredService<ISqlRepository>();

                using (var db = GetInstallationContext(databases))
                {
                    foreach (var tenant in db.Tenant.ToList())
                    {
                        try
                        {
                            var dbConfig = new DbConfig(null, null, databases) {ConnectionString = tenant.DBConnectionString, DatabaseType = tenant.DBType};
                            using (var tenantDbContext = new TenantDBContext(dbConfig, null))
                            {
                                if (install.DatabaseType == "SqlServer" || install.DatabaseType == "LocalDB")
                                {
                                    UpgradeSqlServer(sql, tenant.DBConnectionString, false);
                                }

                                // Push latest model into database
                                tenantDbContext.Database.Migrate();
                                result.Success = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Message = ex.Message;
                        }

                        // execute any version specific upgrade logic
                        var version = tenant.Version;
                        var index = Array.FindIndex(versions, item => item == version);
                        if (index != (versions.Length - 1))
                        {
                            if (index == -1) index = 0;
                            for (var i = index; i < versions.Length; i++)
                            {
                                upgrades.Upgrade(tenant, versions[i]);
                            }
                            tenant.Version = versions[versions.Length - 1];
                            db.Entry(tenant).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                result.Success = true;
            }

            return result;
        }

        private Installation MigrateModules(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var moduleDefinitions = scope.ServiceProvider.GetRequiredService<IModuleDefinitionRepository>();
                var sql = scope.ServiceProvider.GetRequiredService<ISqlRepository>();
                var databases = scope.ServiceProvider.GetServices<IOqtaneDatabase>();

                foreach (var moduleDefinition in moduleDefinitions.GetModuleDefinitions())
                {
                    if (!string.IsNullOrEmpty(moduleDefinition.ReleaseVersions) && !string.IsNullOrEmpty(moduleDefinition.ServerManagerType))
                    {
                        var moduleType = Type.GetType(moduleDefinition.ServerManagerType);
                        if (moduleType != null)
                        {
                            var versions = moduleDefinition.ReleaseVersions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            using (var db = GetInstallationContext(databases))
                            {
                                foreach (var tenant in db.Tenant.ToList())
                                {
                                    var index = Array.FindIndex(versions, item => item == moduleDefinition.Version);
                                    if (tenant.Name == install.TenantName && install.TenantName != TenantNames.Master)
                                    {
                                        index = -1;
                                    }
                                    if (index != (versions.Length - 1))
                                    {
                                        if (index == -1) index = 0;
                                        for (var i = index; i < versions.Length; i++)
                                        {
                                            try
                                            {
                                                if (moduleType.GetInterface("IInstallable") != null)
                                                {
                                                    var moduleObject = ActivatorUtilities.CreateInstance(scope.ServiceProvider, moduleType) as IInstallable;
                                                    moduleObject?.Install(tenant, versions[i]);
                                                }
                                                else
                                                {
                                                    sql.ExecuteScript(tenant, moduleType.Assembly, Utilities.GetTypeName(moduleDefinition.ModuleDefinitionName) + "." + versions[i] + ".sql");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                result.Message = "An Error Occurred Installing " + moduleDefinition.Name + " Version " + versions[i] + " - " + ex.Message;
                                            }
                                        }
                                    }
                                }
                                if (string.IsNullOrEmpty(result.Message) && moduleDefinition.Version != versions[versions.Length - 1])
                                {
                                    moduleDefinition.Version = versions[versions.Length - 1];
                                    db.Entry(moduleDefinition).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                result.Success = true;
            }

            return result;
        }

        private Installation CreateSite(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            if (!string.IsNullOrEmpty(install.TenantName) && !string.IsNullOrEmpty(install.Aliases) && !string.IsNullOrEmpty(install.SiteName))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    // use the SiteState to set the Alias explicitly so the tenant can be resolved
                    var aliases = scope.ServiceProvider.GetRequiredService<IAliasRepository>();
                    var firstAlias = install.Aliases.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    var alias = aliases.GetAliases().FirstOrDefault(item => item.Name == firstAlias);
                    var siteState = scope.ServiceProvider.GetRequiredService<SiteState>();
                    siteState.Alias = alias;

                    var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                    var site = sites.GetSites().FirstOrDefault(item => item.Name == install.SiteName);
                    if (site == null)
                    {
                        var tenants = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        var roles = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
                        var userRoles = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
                        var folders = scope.ServiceProvider.GetRequiredService<IFolderRepository>();
                        var log = scope.ServiceProvider.GetRequiredService<ILogManager>();
                        var identityUserManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                        var tenant = tenants.GetTenants().FirstOrDefault(item => item.Name == install.TenantName);

                        site = new Site
                        {
                            TenantId = tenant.TenantId,
                            Name = install.SiteName,
                            LogoFileId = null,
                            DefaultThemeType = install.DefaultTheme,
                            DefaultLayoutType = install.DefaultLayout,
                            DefaultContainerType = install.DefaultContainer,
                            SiteTemplateType = install.SiteTemplate
                        };
                        site = sites.AddSite(site);

                        var identityUser = identityUserManager.FindByNameAsync(UserNames.Host).GetAwaiter().GetResult();
                        if (identityUser == null)
                        {
                            identityUser = new IdentityUser {UserName = UserNames.Host, Email = install.HostEmail, EmailConfirmed = true};
                            var create = identityUserManager.CreateAsync(identityUser, install.HostPassword).GetAwaiter().GetResult();
                            if (create.Succeeded)
                            {
                                var user = new User
                                {
                                    SiteId = site.SiteId,
                                    Username = UserNames.Host,
                                    Password = install.HostPassword,
                                    Email = install.HostEmail,
                                    DisplayName = install.HostName,
                                    LastIPAddress = "",
                                    LastLoginOn = null
                                };

                                user = users.AddUser(user);
                                var hostRoleId = roles.GetRoles(user.SiteId, true).FirstOrDefault(item => item.Name == RoleNames.Host)?.RoleId ?? 0;
                                var userRole = new UserRole {UserId = user.UserId, RoleId = hostRoleId, EffectiveDate = null, ExpiryDate = null};
                                userRoles.AddUserRole(userRole);

                                // add user folder
                                var folder = folders.GetFolder(user.SiteId, Utilities.PathCombine("Users", Path.DirectorySeparatorChar.ToString()));
                                if (folder != null)
                                {
                                    folders.AddFolder(new Folder
                                    {
                                        SiteId = folder.SiteId,
                                        ParentId = folder.FolderId,
                                        Name = "My Folder",
                                        Path = Utilities.PathCombine(folder.Path, user.UserId.ToString(), Path.DirectorySeparatorChar.ToString()),
                                        Order = 1,
                                        IsSystem = true,
                                        Permissions = new List<Permission>
                                        {
                                            new Permission(PermissionNames.Browse, user.UserId, true),
                                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                                            new Permission(PermissionNames.Edit, user.UserId, true),
                                        }.EncodePermissions(),
                                    });
                                }
                            }
                        }

                        foreach (var aliasName in install.Aliases.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                        {
                            alias = aliases.GetAliases().FirstOrDefault(item => item.Name == aliasName);
                            if (alias != null)
                            {
                                alias.SiteId = site.SiteId;
                                aliases.UpdateAlias(alias);
                            }
                        }

                        tenant.Version = Constants.Version;
                        tenants.UpdateTenant(tenant);

                        if (site != null) log.Log(site.SiteId, LogLevel.Trace, this, LogFunction.Create, "Site Created {Site}", site);
                    }
                }
            }

            result.Success = true;

            return result;
        }

        public void AddOrUpdateAppSetting<T>(string sectionPathKey, T value)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = File.ReadAllText(filePath);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);

                SetValueRecursively(sectionPathKey, jsonObj, value);

                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(filePath, output);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing app settings | {0}", ex);
            }
        }

        private string DenormalizeConnectionString(string connectionString)
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            connectionString = connectionString.Replace(dataDirectory ?? String.Empty, "|DataDirectory|");
            return connectionString;
        }

        private InstallationContext GetInstallationContext(IEnumerable<IOqtaneDatabase> databases)
        {
            var databaseType = _config.GetSection(SettingKeys.DatabaseSection)[SettingKeys.DatabaseTypeKey];
            var connectionString = NormalizeConnectionString(_config.GetConnectionString(SettingKeys.ConnectionStringKey));

            return new InstallationContext(databases.Single(d => d.Name == databaseType), connectionString);
        }

        private string GetInstallationConfig(string key, string defaultValue)
        {
            var value = _config.GetSection(SettingKeys.InstallationSection).GetValue(key, defaultValue);
            // double fallback to default value - allow hold sample keys in config
            if (string.IsNullOrEmpty(value)) value = defaultValue;
            return value;
        }

        private string NormalizeConnectionString(string connectionString)
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            connectionString = connectionString.Replace("|DataDirectory|", dataDirectory);
            return connectionString;
        }

        private void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
        {
            // split the string at the first ':' character
            var remainingSections = sectionPathKey.Split(":", 2);

            var currentSection = remainingSections[0];
            if (remainingSections.Length > 1)
            {
                // continue with the process, moving down the tree
                var nextSection = remainingSections[1];
                SetValueRecursively(nextSection, jsonObj[currentSection], value);
            }
            else
            {
                // we've got to the end of the tree, set the value
                jsonObj[currentSection] = value;
            }
        }

        public void UpdateConnectionString(string connectionString)
        {
            connectionString = DenormalizeConnectionString(connectionString);
            if (_config.GetConnectionString(SettingKeys.ConnectionStringKey) != connectionString)
            {
                AddOrUpdateAppSetting($"{SettingKeys.ConnectionStringsSection}:{SettingKeys.ConnectionStringKey}", connectionString);
                _config.Reload();
            }
        }

        public void UpdateDatabaseType(string databaseType)
        {
            AddOrUpdateAppSetting($"{SettingKeys.DatabaseSection}:{SettingKeys.DatabaseTypeKey}", databaseType);
            _config.Reload();
        }

        public void UpgradeSqlServer(ISqlRepository sql, string connectionString, bool isMaster)
        {
            var script = (isMaster) ? "MigrateMaster.sql" : "MigrateTenant.sql";

            sql.ExecuteScript(connectionString, Assembly.GetExecutingAssembly(), script);
        }
    }
}
