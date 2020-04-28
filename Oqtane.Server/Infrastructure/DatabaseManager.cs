using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using File = System.IO.File;

namespace Oqtane.Infrastructure
{
    public class DatabaseManager
    {
        private readonly IConfigurationRoot _config;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private bool _isInstalled;


        public DatabaseManager(IConfigurationRoot config, IServiceScopeFactory serviceScopeFactory)
        {
            _config = config;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public string Message { get; set; }

        public void StartupMigration()
        {
            var defaultConnectionString = _config.GetConnectionString(SettingKeys.ConnectionStringKey);
            var defaultAlias = GetInstallationConfig(SettingKeys.DefaultAliasKey, string.Empty);
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            
            //create data directory if does not exists
            if (!Directory.Exists(dataDirectory)) Directory.CreateDirectory(dataDirectory);

            // if no values specified, fallback to IDE installer
            if (string.IsNullOrEmpty(defaultConnectionString))
            {
                IsInstalled = false;
                return;
            }

            var freshInstall = !IsMasterInstalled(defaultConnectionString);
            var password = GetInstallationConfig(SettingKeys.HostPasswordKey, String.Empty);
            var email = GetInstallationConfig(SettingKeys.HostEmailKey, String.Empty);
            if (freshInstall && (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(defaultAlias)))
            {
                IsInstalled = false;
                Message = "Incomplete startup install configuration";
                return;
            }

            var result = MasterMigration(defaultConnectionString, defaultAlias, null, true);
            IsInstalled = result.Success;

            if (result.Success)
            {
                WriteVersionInfo(defaultConnectionString);
                TenantMigration(defaultConnectionString, dataDirectory);
            }
            
            if (_isInstalled && !IsDefaultSiteInstalled(defaultConnectionString))
            {
                BuildDefaultSite(password,email);
            }
        }

        public bool IsInstalled
        {
            get
            {
                if (!_isInstalled) _isInstalled = CheckInstallState();

                return _isInstalled;
            }
            set => _isInstalled = value;
        }
        
        private bool CheckInstallState()
        {
            var defaultConnectionString = _config.GetConnectionString(SettingKeys.ConnectionStringKey);
            var result = !string.IsNullOrEmpty(defaultConnectionString);
            if (result)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MasterDBContext>();
                    result = dbContext.Database.CanConnect();
                }
                if (result)
                {
                    //I think this is obsolete now and not accurate, maybe check presence of some table, Version ???
                    var dbUpgradeConfig = DeployChanges
                        .To
                        .SqlDatabase(defaultConnectionString)
                        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.Contains("Master"));

                    result = !dbUpgradeConfig.Build().IsUpgradeRequired();
                    if (!result) Message = "Master Installation Scripts Have Not Been Executed";
                }
                else
                {
                    Message = "Database is not available";
                }
            }
            else
            {
                Message = "Connection string is empty";
            }
            return result;
        }


        public static string NormalizeConnectionString(string connectionString, string dataDirectory)
        {
            connectionString = connectionString
                .Replace("|DataDirectory|", dataDirectory);
            return connectionString;
        }

        public static Installation InstallDatabase([NotNull] InstallConfig installConfig)
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            var result = new Installation {Success = false, Message = ""};

            var alias = installConfig.Alias;
            var connectionString = NormalizeConnectionString(installConfig.ConnectionString, dataDirectory);

            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(alias))
            {
                result = MasterMigration(connectionString, alias, result, installConfig.IsMaster);
                if (installConfig.IsMaster && result.Success)
                {
                    WriteVersionInfo(connectionString);
                    TenantMigration(connectionString, dataDirectory);
                    UpdateConnectionStringSetting(connectionString);
                }
                return result;
            }

            result = new Installation
            {
                Success = false,
                Message = "Connection string is empty",
            };
            return result;
        }

        private static Installation MasterMigration(string connectionString, string alias, Installation result, bool master)
        {
            if (result == null) result = new Installation {Success = false, Message = string.Empty};

            bool firstInstall;
            try
            {
                // create empty database if does not exists       
                // dbup database creation does not work correctly on localdb databases
                using (var dbc = new DbContext(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options))
                {
                    dbc.Database.EnsureCreated();
                    //check for vanilla db
                    firstInstall = !TableExists(dbc, "SchemaVersions");
                }
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                Console.WriteLine(e);
                return result;
            }
            // when alias is not specified on first install, fallback to ide
            if (firstInstall && string.IsNullOrEmpty(alias)) return result;
           
            var dbUpgradeConfig = DeployChanges
                .To
                .SqlDatabase(connectionString)
                .WithVariable("ConnectionString", connectionString)
                .WithVariable("Alias", alias)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.Contains("Master."));

            var dbUpgrade = dbUpgradeConfig.Build();
            if (!dbUpgrade.IsUpgradeRequired())
            {
                result.Success = true;
                result.Message = string.Empty;
                return result;
            }

            var upgradeResult = dbUpgrade.PerformUpgrade();
            if (!upgradeResult.Successful)
            {
                Console.WriteLine(upgradeResult.Error.Message);
                result.Message = upgradeResult.Error.Message;
            }
            else
            {
                result.Success = true;
            }

            return result;
        }

        private static void ModuleMigration(Assembly assembly, string connectionString)
        {
            Console.WriteLine($"Migrating assembly {assembly.FullName}");
            var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(assembly, s => !s.ToLower().Contains("uninstall.sql")); // scripts must be included as Embedded Resources
            var dbUpgrade = dbUpgradeConfig.Build();
            if (dbUpgrade.IsUpgradeRequired())
            {
                var result = dbUpgrade.PerformUpgrade();
                if (!result.Successful)
                {
                    // TODO: log result.Error.Message - problem is logger is not available here
                }
            }
        }

        private static void WriteVersionInfo(string connectionString)
        {
            using (var db = new InstallationContext(connectionString))
            {
                var version = db.ApplicationVersion.ToList().LastOrDefault();
                if (version == null || version.Version != Constants.Version)
                {
                    version = new ApplicationVersion {Version = Constants.Version, CreatedOn = DateTime.UtcNow};
                    db.ApplicationVersion.Add(version);
                    db.SaveChanges();
                }
            }
        }
        
        private static void TenantMigration(string connectionString, string dataDirectory)
        {
            Console.WriteLine("Tenant migration");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName != null && item.FullName.ToLower().Contains(".module.")).ToArray();

            // get tenants
            using (var db = new InstallationContext(connectionString))
            {
                foreach (var tenant in db.Tenant.ToList())
                {
                    Console.WriteLine($"Migrating tenant {tenant.Name}");
                    connectionString = NormalizeConnectionString(tenant.DBConnectionString, dataDirectory);
                    // upgrade framework
                    var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.Contains("Tenant"));
                    var dbUpgrade = dbUpgradeConfig.Build();
                    if (dbUpgrade.IsUpgradeRequired())
                    {
                        var result = dbUpgrade.PerformUpgrade();
                        if (!result.Successful)
                        {
                            // TODO: log result.Error.Message - problem is logger is not available here
                        }
                    }

                    // iterate through Oqtane module assemblies and execute any database scripts
                    foreach (var assembly in assemblies) ModuleMigration(assembly, connectionString);
                }
            }
        }

        public static void UpdateConnectionStringSetting(string connectionString)
        {
            AddOrUpdateAppSetting($"ConnectionStrings:{SettingKeys.ConnectionStringKey}", connectionString);
        }

        public static void AddOrUpdateAppSetting<T>(string sectionPathKey, T value)
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

        private static void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
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

        public void BuildDefaultSite(string password, string email)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                //Gather required services
                var siteRepository = scope.ServiceProvider.GetRequiredService<ISiteRepository>();

                // Build default site only if no site present
                if (siteRepository.GetSites().Any()) return;

                var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var roles = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
                var userRoles = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
                var folders = scope.ServiceProvider.GetRequiredService<IFolderRepository>();
                var identityUserManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var tenants = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

                var tenant = tenants.GetTenants().First();

                var site = new Site
                {
                    TenantId = tenant.TenantId,
                    Name = "Default Site",
                    LogoFileId = null,
                    DefaultThemeType = GetInstallationConfig(SettingKeys.DefaultThemeKey, Constants.DefaultTheme),
                    DefaultLayoutType = GetInstallationConfig(SettingKeys.DefaultLayoutKey, Constants.DefaultLayout),
                    DefaultContainerType = GetInstallationConfig(SettingKeys.DefaultContainerKey, Constants.DefaultContainer),
                    SiteTemplateType = GetInstallationConfig(SettingKeys.SiteTemplateKey, Constants.DefaultSiteTemplate),
                };
                site = siteRepository.AddSite(site);

                var user = new User
                {
                    SiteId = site.SiteId,
                    Username = Constants.HostUser,
                    Password = password,
                    Email = email,
                    DisplayName = Constants.HostUser
                };
                CreateHostUser(folders, userRoles, roles, users, identityUserManager, user);
                tenant.IsInitialized = true;
                tenants.UpdateTenant(tenant);
            }
        }

        private string GetInstallationConfig(string key, string defaultValue)
        {
            var value = _config.GetSection(SettingKeys.InstallationSection).GetValue(key, defaultValue);
            // double fallback to default value - allow hold sample keys in config
            if (string.IsNullOrEmpty(value)) value = defaultValue;
            return value;
        }
        
        private static void CreateHostUser(IFolderRepository folderRepository, IUserRoleRepository userRoleRepository, IRoleRepository roleRepository, IUserRepository userRepository, UserManager<IdentityUser> identityUserManager, User user)
        {
            var identityUser = new IdentityUser {UserName = user.Username, Email = user.Email, EmailConfirmed = true};
            var result = identityUserManager.CreateAsync(identityUser, user.Password).GetAwaiter().GetResult();

            if (result.Succeeded)
            {
                user.LastLoginOn = null;
                user.LastIPAddress = "";
                var newUser = userRepository.AddUser(user);

                // assign to host role if this is the host user ( initial installation )
                if (user.Username == Constants.HostUser)
                {
                    var hostRoleId = roleRepository.GetRoles(user.SiteId, true).FirstOrDefault(item => item.Name == Constants.HostRole)?.RoleId ?? 0;
                    var userRole = new UserRole {UserId = newUser.UserId, RoleId = hostRoleId, EffectiveDate = null, ExpiryDate = null};
                    userRoleRepository.AddUserRole(userRole);
                }

                // add folder for user
                var folder = folderRepository.GetFolder(user.SiteId, Utilities.PathCombine("Users","\\"));
                if (folder != null)
                    folderRepository.AddFolder(new Folder
                    {
                        SiteId = folder.SiteId,
                        ParentId = folder.FolderId,
                        Name = "My Folder",
                        Path = Utilities.PathCombine(folder.Path, newUser.UserId.ToString(),"\\"),
                        Order = 1,
                        IsSystem = true,
                        Permissions = new List<Permission>
                        {
                            new Permission(PermissionNames.Browse, newUser.UserId, true),
                            new Permission(PermissionNames.View, Constants.AllUsersRole, true),
                            new Permission(PermissionNames.Edit, newUser.UserId, true),
                        }.EncodePermissions(),
                    });
            }
        }

        private static bool IsDefaultSiteInstalled(string connectionString)
        {
            using (var db = new InstallationContext(connectionString))
            {
                return db.Tenant.Any(t => t.IsInitialized);
            }
        }

        private static bool IsMasterInstalled(string connectionString)
        {
            using (var db = new InstallationContext(connectionString))
            {
 
                //check if DbUp was initialized
                return TableExists(db, "SchemaVersions");
            }
        }

        public static bool TableExists(DbContext context, string tableName)
        {
            return TableExists(context, "dbo", tableName);
        }

        public static bool TableExists(DbContext context, string schema, string tableName)
        {
            if (!context.Database.CanConnect()) return false;
            var connection = context.Database.GetDbConnection();

            if (connection.State.Equals(ConnectionState.Closed))
                connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
            SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = @Schema
            AND TABLE_NAME = @TableName";

                var schemaParam = command.CreateParameter();
                schemaParam.ParameterName = "@Schema";
                schemaParam.Value = schema;
                command.Parameters.Add(schemaParam);

                var tableNameParam = command.CreateParameter();
                tableNameParam.ParameterName = "@TableName";
                tableNameParam.Value = tableName;
                command.Parameters.Add(tableNameParam);

                return command.ExecuteScalar() != null;
            }
        }
    }
}
