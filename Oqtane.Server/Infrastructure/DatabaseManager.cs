using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Databases.Interfaces;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using Oqtane.Enums;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
// ReSharper disable UseIndexFromEndExpression

namespace Oqtane.Infrastructure
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly IConfigManager _config;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;
        private readonly IConfigManager _configManager;
        private readonly ILogger<DatabaseManager> _filelogger;

        public DatabaseManager(IConfigManager config, IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment environment, IMemoryCache cache, IConfigManager configManager, ILogger<DatabaseManager> filelogger)
        {
            _config = config;
            _serviceScopeFactory = serviceScopeFactory;
            _environment = environment;
            _cache = cache;
            _configManager = configManager;
            _filelogger = filelogger;
        }

        public Installation IsInstalled()
        {
            var result = new Installation { Success = false, Message = string.Empty };

            if (!string.IsNullOrEmpty(_config.GetConnectionString(SettingKeys.ConnectionStringKey)))
            {
                using (var db = GetInstallationContext())
                {
                    if (db.Database.CanConnect())
                    {
                        try
                        {
                            // verify master database contains a Tenant table ( ie. validate schema is properly provisioned )
                            var provisioned = db.Tenant.Any();
                            result.Success = true;
                        }
                        catch (Exception ex)
                        {
                            result.Message = "Master Database Not Installed Correctly. " + ex.ToString();
                        }
                    }
                    else // cannot connect
                    {
                        try
                        {
                            // get the actual connection error details
                            db.Database.OpenConnection();
                        }
                        catch (Exception ex)
                        {
                            result.Message = "Cannot Connect To Master Database. " + ex.ToString();
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

            ValidateConfiguration();

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

                // on upgrade install the associated Nuget package
                if (!string.IsNullOrEmpty(install.ConnectionString))
                {
                    InstallDatabase(install);
                }

                var installation = IsInstalled();
                if (!installation.Success)
                {
                    install.Aliases = GetInstallationConfig(SettingKeys.DefaultAliasKey, string.Empty);
                    install.HostUsername = GetInstallationConfig(SettingKeys.HostUsernameKey, UserNames.Host);
                    install.HostPassword = GetInstallationConfig(SettingKeys.HostPasswordKey, string.Empty);
                    install.HostEmail = GetInstallationConfig(SettingKeys.HostEmailKey, string.Empty);
                    install.HostName = GetInstallationConfig(SettingKeys.HostNameKey, UserNames.Host);

                    if (!string.IsNullOrEmpty(install.ConnectionString) && !string.IsNullOrEmpty(install.Aliases) && !string.IsNullOrEmpty(install.HostPassword) && !string.IsNullOrEmpty(install.HostEmail))
                    {
                        // silent install
                        install.SiteTemplate = GetInstallationConfig(SettingKeys.SiteTemplateKey, Constants.DefaultSiteTemplate);
                        install.DefaultTheme = GetInstallationConfig(SettingKeys.DefaultThemeKey, Constants.DefaultTheme);
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
                        _filelogger.LogError(Utilities.LogMessage(this, installation.Message));
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
                    }
                    if (string.IsNullOrEmpty(install.DefaultContainer))
                    {
                        install.DefaultContainer = GetInstallationConfig(SettingKeys.DefaultContainerKey, Constants.DefaultContainer);
                    }

                    // add new site
                    if (install.TenantName != TenantNames.Master && install.ConnectionString.Contains("="))
                    {
                        _configManager.AddOrUpdateSetting($"{SettingKeys.ConnectionStringsSection}:{install.TenantName}", install.ConnectionString, false);
                    }
                    if (install.TenantName == TenantNames.Master && !install.ConnectionString.Contains("="))
                    {
                        install.ConnectionString = _config.GetConnectionString(install.ConnectionString);
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
                                    if (result.Success)
                                    {
                                        result = MigrateSites();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private Installation InstallDatabase(InstallConfig install)
        {
            var result = new Installation {Success = false, Message = string.Empty};

            try
            {
                bool installPackages = false;

                // iterate database packages in installation folder
                var packagesFolder = new DirectoryInfo(Path.Combine(_environment.ContentRootPath, Constants.PackagesFolder));
                foreach (var package in packagesFolder.GetFiles("*.nupkg.bak"))
                {
                    // determine if package needs to be upgraded or installed
                    bool upgrade = System.IO.File.Exists(package.FullName.Replace(".nupkg.bak",".log"));
                    if (upgrade || package.Name.StartsWith(Utilities.GetAssemblyName(install.DatabaseType)))
                    {
                        var packageName = Path.Combine(package.DirectoryName, package.Name);
                        packageName = packageName.Substring(0, packageName.IndexOf(".bak"));
                        package.MoveTo(packageName, true);
                        installPackages = true;
                    }
                }
                if (installPackages)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var installationManager = scope.ServiceProvider.GetRequiredService<IInstallationManager>();
                        installationManager.InstallPackages();
                    }
                }

                // load the installation database type (if necessary)
                if (Type.GetType(install.DatabaseType) == null)
                {
                    var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
                    var assembliesFolder = new DirectoryInfo(assemblyPath);
                    var assemblyFile = new FileInfo($"{assembliesFolder}/{Utilities.GetAssemblyName(install.DatabaseType)}.dll");
                    AssemblyLoadContext.Default.LoadOqtaneAssembly(assemblyFile);
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.ToString();
                _filelogger.LogError(Utilities.LogMessage(this, result.Message));
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
                    InstallDatabase(install);

                    var databaseType = install.DatabaseType;

                    // get database type
                    var type = Type.GetType(databaseType);

                    // create database object from type
                    var database = Activator.CreateInstance(type) as IDatabase;

                    // create data directory if does not exist
                    var dataDirectory = AppDomain.CurrentDomain.GetData(Constants.DataDirectory)?.ToString();
                    if (!Directory.Exists(dataDirectory)) Directory.CreateDirectory(dataDirectory ?? String.Empty);

                    var dbOptions = new DbContextOptionsBuilder().UseOqtaneDatabase(database, NormalizeConnectionString(install.ConnectionString)).Options;
                    using (var dbc = new DbContext(dbOptions))
                    {
                        // create empty database if it does not exist
                        dbc.Database.EnsureCreated();
                        result.Success = true;
                    }
                }
                catch (Exception ex)
                {
                    result.Message = "An Error Occurred Creating The Database. This Is Usually Related To Your User Not Having Sufficient Rights To Perform This Operation. Please Note That You Can Also Create The Database Manually Prior To Initiating The Install Wizard. " + ex.ToString();
                    _filelogger.LogError(Utilities.LogMessage(this, result.Message));
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
                    var sql = scope.ServiceProvider.GetRequiredService<ISqlRepository>();

                    var installation = IsInstalled();
                    try
                    {
                        UpdateConnectionString(install.ConnectionString);
                        UpdateDatabaseType(install.DatabaseType);

                        using (var masterDbContext = new MasterDBContext(new DbContextOptions<MasterDBContext>(), null, _config))
                        {
                            AddEFMigrationsHistory(sql, install.ConnectionString, install.DatabaseType, "", true);
                            // push latest model into database
                            masterDbContext.Database.Migrate();
                            result.Success = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = "An Error Occurred Provisioning The Master Database. This Is Usually Related To The Master Database Not Being In A Supported State. " + ex.ToString();
                        _filelogger.LogError(Utilities.LogMessage(this, result.Message));
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
                    using (var db = GetInstallationContext())
                    {
                        Tenant tenant;
                        if (install.IsNewTenant)
                        {
                            tenant = new Tenant
                            {
                                Name = install.TenantName,
                                DBConnectionString = (install.TenantName == TenantNames.Master) ? SettingKeys.ConnectionStringKey : install.TenantName,
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

                        var aliasNames = install.Aliases.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(sValue => sValue.Trim()).ToArray();
                        var firstAlias = aliasNames[0];
                        foreach (var aliasName in aliasNames)
                        {
                            if (tenant != null)
                            {
                                var alias = new Alias
                                {
                                    Name = aliasName,
                                    TenantId = tenant.TenantId,
                                    SiteId = -1,
                                    IsDefault = (aliasName == firstAlias),
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

            result.Success = true;

            return result;
        }

        private Installation MigrateTenants(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            var versions = Constants.ReleaseVersions.Split(',', StringSplitOptions.RemoveEmptyEntries);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var upgrades = scope.ServiceProvider.GetRequiredService<IUpgradeManager>();
                var sql = scope.ServiceProvider.GetRequiredService<ISqlRepository>();
                var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
                var DBContextDependencies = scope.ServiceProvider.GetRequiredService<IDBContextDependencies>();

                using (var db = GetInstallationContext())
                {
                    foreach (var tenant in db.Tenant.ToList())
                    {
                        tenantManager.SetTenant(tenant.TenantId);
                        tenant.DBConnectionString = MigrateConnectionString(db, tenant);
                        try
                        {
                            using (var tenantDbContext = new TenantDBContext(DBContextDependencies))
                            {
                                AddEFMigrationsHistory(sql, _configManager.GetSetting($"{SettingKeys.ConnectionStringsSection}:{tenant.DBConnectionString}", ""), tenant.DBType, tenant.Version, false);
                                // push latest model into database
                                tenantDbContext.Database.Migrate();
                                result.Success = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Message = "An Error Occurred Migrating A Tenant Database. This Is Usually Related To A Tenant Database Not Being In A Supported State. " + ex.ToString();
                            _filelogger.LogError(Utilities.LogMessage(this, result.Message));
                        }

                        // execute any version specific upgrade logic
                        var version = tenant.Version;
                        var index = Array.FindIndex(versions, item => item == version);
                        if (index != (versions.Length - 1))
                        {
                            try
                            {
                                for (var i = (index + 1); i < versions.Length; i++)
                                {
                                    upgrades.Upgrade(tenant, versions[i]);
                                }
                                tenant.Version = versions[versions.Length - 1];
                                db.Entry(tenant).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                result.Message = "An Error Occurred Executing Upgrade Logic. " + ex.ToString();
                                _filelogger.LogError(Utilities.LogMessage(this, result.Message));
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

        private Installation MigrateModules(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var moduleDefinitions = scope.ServiceProvider.GetRequiredService<IModuleDefinitionRepository>();
                var sql = scope.ServiceProvider.GetRequiredService<ISqlRepository>();
                var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();

                foreach (var moduleDefinition in moduleDefinitions.GetModuleDefinitions())
                {
                    if (!string.IsNullOrEmpty(moduleDefinition.ReleaseVersions))
                    {
                        var versions = moduleDefinition.ReleaseVersions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        using (var db = GetInstallationContext())
                        {
                            if (!string.IsNullOrEmpty(moduleDefinition.ServerManagerType))
                            {
                                var moduleType = Type.GetType(moduleDefinition.ServerManagerType);
                                if (moduleType != null)
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
                                            for (var i = (index + 1); i < versions.Length; i++)
                                            {
                                                try
                                                {
                                                    if (moduleType.GetInterface("IInstallable") != null)
                                                    {
                                                        tenantManager.SetTenant(tenant.TenantId);
                                                        var moduleObject = ActivatorUtilities.CreateInstance(scope.ServiceProvider, moduleType) as IInstallable;
                                                        if (moduleObject == null || !moduleObject.Install(tenant, versions[i]))
                                                        {
                                                            result.Message = "An Error Occurred Executing IInstallable Interface For " + moduleDefinition.ServerManagerType;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!sql.ExecuteScript(tenant, moduleType.Assembly, Utilities.GetTypeName(moduleDefinition.ModuleDefinitionName) + "." + versions[i] + ".sql"))
                                                        {
                                                            result.Message = "An Error Occurred Executing Database Script " + Utilities.GetTypeName(moduleDefinition.ModuleDefinitionName) + "." + versions[i] + ".sql";
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    result.Message = "An Error Occurred Installing " + moduleDefinition.Name + " Version " + versions[i] + " - " + ex.ToString();
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(result.Message) && moduleDefinition.Version != versions[versions.Length - 1])
                            {
                                // get module definition from database to retain user customizable property values
                                var moduledef = db.ModuleDefinition.AsNoTracking().FirstOrDefault(item => item.ModuleDefinitionId == moduleDefinition.ModuleDefinitionId);
                                moduleDefinition.Name = moduledef.Name;
                                moduleDefinition.Description = moduledef.Description;
                                moduleDefinition.Categories = moduledef.Categories;
                                // update version
                                moduleDefinition.Version = versions[versions.Length - 1];
                                db.Entry(moduleDefinition).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                result.Success = true;
            }
            else
            {
                _filelogger.LogError(Utilities.LogMessage(this, result.Message));
            }

            return result;
        }

        private Installation CreateSite(InstallConfig install)
        {
            var result = new Installation { Success = false, Message = string.Empty };

            if (!string.IsNullOrEmpty(install.TenantName) && !string.IsNullOrEmpty(install.Aliases) && !string.IsNullOrEmpty(install.SiteName))
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        // set the alias explicitly so the tenant can be resolved
                        var aliases = scope.ServiceProvider.GetRequiredService<IAliasRepository>();
                        var aliasNames = install.Aliases.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(sValue => sValue.Trim()).ToArray();
                        var firstAlias = aliasNames[0];
                        var alias = aliases.GetAliases().FirstOrDefault(item => item.Name == firstAlias);
                        var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
                        tenantManager.SetAlias(alias);

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
                                FaviconFileId = null,
                                PwaIsEnabled = false,
                                PwaAppIconFileId = null,
                                PwaSplashIconFileId = null,
                                AllowRegistration = false,
                                CaptureBrokenUrls = true,
                                VisitorTracking = true,
                                DefaultThemeType = (!string.IsNullOrEmpty(install.DefaultTheme)) ? install.DefaultTheme : Constants.DefaultTheme,
                                DefaultContainerType = (!string.IsNullOrEmpty(install.DefaultContainer)) ? install.DefaultContainer : Constants.DefaultContainer,
                                AdminContainerType = (!string.IsNullOrEmpty(install.DefaultAdminContainer)) ? install.DefaultAdminContainer : Constants.DefaultAdminContainer,
                                SiteTemplateType = install.SiteTemplate,
                                Runtime = (!string.IsNullOrEmpty(install.Runtime)) ? install.Runtime : _configManager.GetSection("Runtime").Value,
                                RenderMode = (!string.IsNullOrEmpty(install.RenderMode)) ? install.RenderMode : _configManager.GetSection("RenderMode").Value,
                            };
                            site = sites.AddSite(site);

                            if (!string.IsNullOrEmpty(install.HostUsername))
                            {
                                var identityUser = identityUserManager.FindByNameAsync(install.HostUsername).GetAwaiter().GetResult();
                                if (identityUser == null)
                                {
                                    identityUser = new IdentityUser { UserName = install.HostUsername, Email = install.HostEmail, EmailConfirmed = true };
                                    var create = identityUserManager.CreateAsync(identityUser, install.HostPassword).GetAwaiter().GetResult();
                                    if (create.Succeeded)
                                    {
                                        var user = new User
                                        {
                                            SiteId = site.SiteId,
                                            Username = install.HostUsername,
                                            Password = install.HostPassword,
                                            Email = install.HostEmail,
                                            DisplayName = install.HostName,
                                            LastIPAddress = "",
                                            LastLoginOn = null
                                        };
                                        user = users.AddUser(user);

                                        // add host role
                                        var hostRoleId = roles.GetRoles(user.SiteId, true).FirstOrDefault(item => item.Name == RoleNames.Host)?.RoleId ?? 0;
                                        var userRole = new UserRole { UserId = user.UserId, RoleId = hostRoleId, EffectiveDate = null, ExpiryDate = null };
                                        userRoles.AddUserRole(userRole);
                                    }
                                }
                            }

                            foreach (var aliasName in aliasNames)
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

                            if (site != null) log.Log(site.SiteId, Shared.LogLevel.Information, this, LogFunction.Create, "Site Created {Site}", site);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Message = "An Error Occurred Creating Site. " + ex.ToString();
                }
            }

            if (string.IsNullOrEmpty(result.Message))
            {
                result.Success = true;
            }
            else
            {
                _filelogger.LogError(Utilities.LogMessage(this, result.Message));
            }

            return result;
        }

        private Installation MigrateSites()
        {
            var result = new Installation { Success = false, Message = string.Empty };

            // get site upgrades
            Dictionary<string, Type> siteupgrades = new Dictionary<string, Type>();
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes(typeof(ISiteMigration)))
                {
                    if (Attribute.IsDefined(type, typeof(SiteMigrationAttribute)))
                    {
                        var attribute = (SiteMigrationAttribute)Attribute.GetCustomAttribute(type, typeof(SiteMigrationAttribute));
                        siteupgrades.Add(attribute.AliasName + " " + attribute.Version, type);
                    }
                }
            }

            // execute site upgrades
            if (siteupgrades.Count > 0)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var aliases = scope.ServiceProvider.GetRequiredService<IAliasRepository>();
                    var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
                    var sites = scope.ServiceProvider.GetRequiredService<ISiteRepository>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogManager>();

                    foreach (var alias in aliases.GetAliases().ToList().Where(item => item.IsDefault))
                    {
                        foreach (var upgrade in siteupgrades)
                        {
                            var aliasname = upgrade.Key.Split(' ').First();
                            // in the future this equality condition could use RegEx to allow for more flexible matching
                            if (string.Equals(alias.Name, aliasname, StringComparison.OrdinalIgnoreCase))
                            {
                                tenantManager.SetTenant(alias.TenantId);
                                var site = sites.GetSites().FirstOrDefault(item => item.SiteId == alias.SiteId);
                                if (site != null)
                                {
                                    var version = upgrade.Key.Split(' ').Last();
                                    if (string.IsNullOrEmpty(site.Version) || Version.Parse(version) > Version.Parse(site.Version))
                                    {
                                        try
                                        {
                                            var obj = ActivatorUtilities.CreateInstance(scope.ServiceProvider, upgrade.Value) as ISiteMigration;
                                            if (obj != null)
                                            {
                                                obj.Up(site, alias);
                                                site.Version = version;
                                                sites.UpdateSite(site);
                                                logger.Log(alias.SiteId, Shared.LogLevel.Information, "Site Migration", LogFunction.Other, "Site Migrated Successfully To Version {version} For {Alias}", version, alias.Name);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Log(alias.SiteId, Shared.LogLevel.Error, "Site Migration", LogFunction.Other, ex, "An Error Occurred Executing Site Migration {Type} For {Alias} And Version {Version}", upgrade.Value, alias.Name, version);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            result.Success = true;
            return result;
        }

        private string DenormalizeConnectionString(string connectionString)
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData(Constants.DataDirectory)?.ToString();
            connectionString = connectionString.Replace(dataDirectory ?? String.Empty, $"|{Constants.DataDirectory}|");
            return connectionString;
        }

        private InstallationContext GetInstallationContext()
        {
            var connectionString = NormalizeConnectionString(_config.GetConnectionString(SettingKeys.ConnectionStringKey));
            var databaseType = _config.GetSection(SettingKeys.DatabaseSection)[SettingKeys.DatabaseTypeKey];

            IDatabase database = null;
            if (!string.IsNullOrEmpty(databaseType))
            {
                var type = Type.GetType(databaseType);
                database = Activator.CreateInstance(type) as IDatabase;
            }

            return new InstallationContext(database, connectionString);
        }

        private string GetInstallationConfig(string key, string defaultValue)
        {
            return _configManager.GetSetting(SettingKeys.InstallationSection, key, defaultValue);
        }

        private string NormalizeConnectionString(string connectionString)
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData(Constants.DataDirectory)?.ToString();
            connectionString = connectionString.Replace($"|{Constants.DataDirectory}|", dataDirectory);
            return connectionString;
        }

        public void UpdateConnectionString(string connectionString)
        {
            connectionString = DenormalizeConnectionString(connectionString);
            if (_config.GetConnectionString(SettingKeys.ConnectionStringKey) != connectionString)
            {
                _configManager.AddOrUpdateSetting($"{SettingKeys.ConnectionStringsSection}:{SettingKeys.ConnectionStringKey}", connectionString, true);
            }
        }

        public void UpdateDatabaseType(string databaseType)
        {
            _configManager.AddOrUpdateSetting($"{SettingKeys.DatabaseSection}:{SettingKeys.DatabaseTypeKey}", databaseType, true);
        }

        public void AddEFMigrationsHistory(ISqlRepository sql, string connectionString, string databaseType, string version, bool isMaster)
        {
            // in version 2.1.0 the __EFMigrationsHistory tables were introduced and must be added to existing SQL Server installations
            if ((isMaster || (version != null && Version.Parse(version).CompareTo(Version.Parse("2.1.0")) < 0)) && databaseType == Constants.DefaultDBType)
            {
                var script = (isMaster) ? "MigrateMaster.sql" : "MigrateTenant.sql";

                var query = sql.GetScriptFromAssembly(Assembly.GetExecutingAssembly(), script);
                query = query.Replace("{{Version}}", Constants.Version);

                sql.ExecuteNonQuery(connectionString, databaseType, query);
            }
        }

        public string MigrateConnectionString(InstallationContext db, Tenant tenant)
        {
            // migrate connection strings from the Tenant table to appsettings
            if (tenant.DBConnectionString.Contains("="))
            {
                var defaultConnection = _configManager.GetConnectionString(SettingKeys.ConnectionStringKey);
                if (tenant.DBConnectionString == defaultConnection)
                {
                    tenant.DBConnectionString = SettingKeys.ConnectionStringKey;
                }
                else
                {
                    _configManager.AddOrUpdateSetting($"{SettingKeys.ConnectionStringsSection}:{tenant.Name}", tenant.DBConnectionString, false);
                    tenant.DBConnectionString = tenant.Name;
                }
                db.Entry(tenant).State = EntityState.Modified;
                db.SaveChanges();
            }
            return tenant.DBConnectionString;
        }

        private void ValidateConfiguration()
        {
            if (_configManager.GetSetting(SettingKeys.DatabaseSection, SettingKeys.DatabaseTypeKey, "") == "")
            {
                _configManager.AddOrUpdateSetting($"{SettingKeys.DatabaseSection}:{SettingKeys.DatabaseTypeKey}", Constants.DefaultDBType, true);
            }
            if (!_configManager.GetSection(SettingKeys.AvailableDatabasesSection).Exists())
            {
                string databases = "[";
                databases += "{ \"Name\": \"LocalDB\", \"ControlType\": \"Oqtane.Installer.Controls.LocalDBConfig, Oqtane.Client\", \"DBTYpe\": \"Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer\" },";
                databases += "{ \"Name\": \"SQL Server\", \"ControlType\": \"Oqtane.Installer.Controls.SqlServerConfig, Oqtane.Client\", \"DBTYpe\": \"Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer\" },";
                databases += "{ \"Name\": \"SQLite\", \"ControlType\": \"Oqtane.Installer.Controls.SqliteConfig, Oqtane.Client\", \"DBTYpe\": \"Oqtane.Database.Sqlite.SqliteDatabase, Oqtane.Database.Sqlite\" },";
                databases += "{ \"Name\": \"MySQL\", \"ControlType\": \"Oqtane.Installer.Controls.MySQLConfig, Oqtane.Client\", \"DBTYpe\": \"Oqtane.Database.MySQL.SqlServerDatabase, Oqtane.Database.MySQL\" },";
                databases += "{ \"Name\": \"PostgreSQL\", \"ControlType\": \"Oqtane.Installer.Controls.PostgreSQLConfig, Oqtane.Client\", \"DBTYpe\": \"Oqtane.Database.PostgreSQL.PostgreSQLDatabase, Oqtane.Database.PostgreSQL\" }";
                databases += "]";
                _configManager.AddOrUpdateSetting(SettingKeys.AvailableDatabasesSection, databases, true);
            }
        }
    }
}
