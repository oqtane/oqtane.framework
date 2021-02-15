using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp;
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
using File = System.IO.File;

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

        public bool IsInstalled()
        {
            var defaultConnectionString = NormalizeConnectionString(_config.GetConnectionString(SettingKeys.ConnectionStringKey));
            var result = !string.IsNullOrEmpty(defaultConnectionString);
            if (result)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<MasterDBContext>();
                    result = db.Database.CanConnect();
                    if (result)
                    {
                        try
                        {
                            result = db.Tenant.Any();
                        }
                        catch
                        {
                            result = false;
                        }
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
                install = new InstallConfig { ConnectionString = _config.GetConnectionString(SettingKeys.ConnectionStringKey), TenantName = TenantNames.Master, IsNewTenant = false };

                if (!IsInstalled())
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
                    if (!Directory.Exists(dataDirectory)) Directory.CreateDirectory(dataDirectory);

                    using (var dbc = new DbContext(new DbContextOptionsBuilder().UseSqlServer(NormalizeConnectionString(install.ConnectionString)).Options))
                    {
                        // create empty database if it does not exist       
                        dbc.Database.EnsureCreated();
                        result.Success = true;
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
                MigrateScriptNamingConvention("Master", install.ConnectionString);

                var upgradeConfig = DeployChanges
                .To
                .SqlDatabase(NormalizeConnectionString(install.ConnectionString))
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.Contains("Master.") && s.EndsWith(".sql",StringComparison.OrdinalIgnoreCase));

                var upgrade = upgradeConfig.Build();
                if (upgrade.IsUpgradeRequired())
                {
                    var upgradeResult = upgrade.PerformUpgrade();
                    result.Success = upgradeResult.Successful;
                    if (!result.Success)
                    {
                        result.Message = upgradeResult.Error.Message;
                    }
                }
                else
                {
                    result.Success = true;
                }

                if (result.Success)
                {
                    UpdateConnectionString(install.ConnectionString);
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
                using (var db = new InstallationContext(NormalizeConnectionString(_config.GetConnectionString(SettingKeys.ConnectionStringKey)))) 
                {
                    Tenant tenant;
                    if (install.IsNewTenant)
                    {
                        tenant = new Tenant { Name = install.TenantName, DBConnectionString = DenormalizeConnectionString(install.ConnectionString), CreatedBy = "", CreatedOn = DateTime.UtcNow, ModifiedBy = "", ModifiedOn = DateTime.UtcNow };
                        db.Tenant.Add(tenant);
                        db.SaveChanges();
                        _cache.Remove("tenants");
                    }
                    else
                    {
                        tenant = db.Tenant.FirstOrDefault(item => item.Name == install.TenantName);
                    }

                    foreach (string aliasname in install.Aliases.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var alias = new Alias { Name = aliasname, TenantId = tenant.TenantId, SiteId = -1, CreatedBy = "", CreatedOn = DateTime.UtcNow, ModifiedBy = "", ModifiedOn = DateTime.UtcNow };
                        db.Alias.Add(alias);
                        db.SaveChanges();
                    }
                    _cache.Remove("aliases");
                }
            }

            result.Success = true;

            return result;
        }

        private Installation MigrateTenants(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            string[] versions = Constants.ReleaseVersions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var upgrades = scope.ServiceProvider.GetRequiredService<IUpgradeManager>();
  
                using (var db = new InstallationContext(NormalizeConnectionString(_config.GetConnectionString(SettingKeys.ConnectionStringKey))))
                {
                    foreach (var tenant in db.Tenant.ToList())
                    {
                        MigrateScriptNamingConvention("Tenant", tenant.DBConnectionString);

                        var upgradeConfig = DeployChanges.To.SqlDatabase(NormalizeConnectionString(tenant.DBConnectionString))
                            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.Contains("Tenant.") && s.EndsWith(".sql", StringComparison.OrdinalIgnoreCase));

                        var upgrade = upgradeConfig.Build();
                        if (upgrade.IsUpgradeRequired())
                        {
                            var upgradeResult = upgrade.PerformUpgrade();
                            result.Success = upgradeResult.Successful;
                            if (!result.Success)
                            {
                                result.Message = upgradeResult.Error.Message;
                            }
                        }

                        // execute any version specific upgrade logic
                        string version = tenant.Version;
                        int index = Array.FindIndex(versions, item => item == version);
                        if (index != (versions.Length - 1))
                        {
                            if (index == -1) index = 0;
                            for (int i = index; i < versions.Length; i++)
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
                var moduledefinitions = scope.ServiceProvider.GetRequiredService<IModuleDefinitionRepository>();
                var sql = scope.ServiceProvider.GetRequiredService<ISqlRepository>();
                foreach (var moduledefinition in moduledefinitions.GetModuleDefinitions())
                {
                    if (!string.IsNullOrEmpty(moduledefinition.ReleaseVersions) && !string.IsNullOrEmpty(moduledefinition.ServerManagerType))
                    {
                        Type moduletype = Type.GetType(moduledefinition.ServerManagerType);
                        if (moduletype != null)
                        {
                            string[] versions = moduledefinition.ReleaseVersions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            using (var db = new InstallationContext(NormalizeConnectionString(_config.GetConnectionString(SettingKeys.ConnectionStringKey))))
                            {
                                foreach (var tenant in db.Tenant.ToList())
                                {
                                    int index = Array.FindIndex(versions, item => item == moduledefinition.Version);
                                    if (tenant.Name == install.TenantName && install.TenantName != TenantNames.Master)
                                    {
                                        index = -1;
                                    }
                                    if (index != (versions.Length - 1))
                                    {
                                        if (index == -1) index = 0;
                                        for (int i = index; i < versions.Length; i++)
                                        {
                                            try
                                            {
                                                if (moduletype.GetInterface("IInstallable") != null)
                                                {
                                                    var moduleobject = ActivatorUtilities.CreateInstance(scope.ServiceProvider, moduletype);
                                                    ((IInstallable)moduleobject).Install(tenant, versions[i]);
                                                }
                                                else
                                                {
                                                    sql.ExecuteScript(tenant, moduletype.Assembly, Utilities.GetTypeName(moduledefinition.ModuleDefinitionName) + "." + versions[i] + ".sql");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                result.Message = "An Error Occurred Installing " + moduledefinition.Name + " Version " + versions[i] + " - " + ex.Message.ToString();
                                            }
                                        }
                                    }
                                }
                                if (string.IsNullOrEmpty(result.Message) && moduledefinition.Version != versions[versions.Length - 1])
                                {
                                    moduledefinition.Version = versions[versions.Length - 1];
                                    db.Entry(moduledefinition).State = EntityState.Modified;
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
                    string firstalias = install.Aliases.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    var alias = aliases.GetAliases().FirstOrDefault(item => item.Name == firstalias);
                    var siteState = scope.ServiceProvider.GetRequiredService<SiteState>();
                    siteState.Alias = alias;

                    var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                    var site = sites.GetSites().FirstOrDefault(item => item.Name == install.SiteName);
                    if (site == null)
                    {
                        var tenants = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        var roles = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
                        var userroles = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
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

                        IdentityUser identityUser = identityUserManager.FindByNameAsync(UserNames.Host).GetAwaiter().GetResult();
                        if (identityUser == null)
                        {
                            identityUser = new IdentityUser { UserName = UserNames.Host, Email = install.HostEmail, EmailConfirmed = true };
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
                                var userRole = new UserRole { UserId = user.UserId, RoleId = hostRoleId, EffectiveDate = null, ExpiryDate = null };
                                userroles.AddUserRole(userRole);

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

                        foreach (string aliasname in install.Aliases.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            alias = aliases.GetAliases().FirstOrDefault(item => item.Name == aliasname);
                            alias.SiteId = site.SiteId;
                            aliases.UpdateAlias(alias);
                        }

                        tenant.Version = Constants.Version;
                        tenants.UpdateTenant(tenant);

                        log.Log(site.SiteId, LogLevel.Trace, this, LogFunction.Create, "Site Created {Site}", site);
                    }
                }
            }

            result.Success = true;

            return result;
        }

        private string NormalizeConnectionString(string connectionString)
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            connectionString = connectionString.Replace("|DataDirectory|", dataDirectory);
            return connectionString;
        }

        private string DenormalizeConnectionString(string connectionString)
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            connectionString = connectionString.Replace(dataDirectory, "|DataDirectory|");
            return connectionString;
        }

        public void UpdateConnectionString(string connectionString)
        {
            connectionString = DenormalizeConnectionString(connectionString);
            if (_config.GetConnectionString(SettingKeys.ConnectionStringKey) != connectionString)
            {
                AddOrUpdateAppSetting($"ConnectionStrings:{SettingKeys.ConnectionStringKey}", connectionString);
                _config.Reload();
            }
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

        private string GetInstallationConfig(string key, string defaultValue)
        {
            var value = _config.GetSection(SettingKeys.InstallationSection).GetValue(key, defaultValue);
            // double fallback to default value - allow hold sample keys in config
            if (string.IsNullOrEmpty(value)) value = defaultValue;
            return value;
        }

        private void MigrateScriptNamingConvention(string scriptType, string connectionString)
        {
            // migrate to new naming convention for scripts
            var migrateConfig = DeployChanges.To.SqlDatabase(NormalizeConnectionString(connectionString))
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s == scriptType + ".00.00.00.00.sql");
            var migrate = migrateConfig.Build();
            if (migrate.IsUpgradeRequired())
            {
                migrate.PerformUpgrade();
            }
        }

    }
}
