using System;
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
using Oqtane.Controllers;
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
            var defaultConnectionString = _config.GetConnectionString("DefaultConnection");
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
                    Message = "Database is not avaiable";
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
            //.Replace(@"\", @"\\");
            return connectionString;
        }

        public static Installation InstallDatabase([NotNull] InstallConfig installConfig)
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            var result = new Installation {Success = false, Message = ""};

            var alias = installConfig.Alias;
            var connectionString = NormalizeConnectionString(installConfig.ConnectionString, dataDirectory);

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(alias))
            {
                result = new Installation
                {
                    Success = false,
                    Message = "Connection string is empty",
                };
                return result;
            }

            result = MasterMigration(connectionString, alias, result, installConfig.IsMaster);
            if (installConfig.IsMaster && result.Success)
            {
                WriteVersionInfo(connectionString);
                TenantMigration(connectionString, dataDirectory);
                UpdateOqtaneSettings(connectionString);
                AddOrUpdateAppSetting("Oqtane:DefaultAlias", alias);
            }

            return result;
        }

        private static Installation MasterMigration(string connectionString, string alias, Installation result, bool master)
        {
            if (result == null) result = new Installation {Success = false, Message = string.Empty};

            try
            {
                // create empty database if does not exists       
                // dbup database creation does not work correctly on localdb databases
                using (var dbc = new DbContext(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options))
                {
                    dbc.Database.EnsureCreated();
                }
            }
            catch (Exception e)
            {
                result = new Installation
                {
                    Success = false,
                    Message = e.Message,
                };
                Console.WriteLine(e);
                return result;
            }

            var dbUpgradeConfig = DeployChanges
                    .To
                    .SqlDatabase(connectionString)
                    .WithVariable("ConnectionString", connectionString)
                    .WithVariable("Alias", alias)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => master || !s.Contains("Master."));

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
            var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(assembly); // scripts must be included as Embedded Resources
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
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(item => item.FullName != null && item.FullName.Contains(".Module.")).ToArray();

            // get tenants
            using (var db = new InstallationContext(connectionString))
            {
                foreach (var tenant in db.Tenant.ToList())
                {
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

        public static void UpdateOqtaneSettings(string connectionString)
        {
            AddOrUpdateAppSetting("ConnectionStrings:DefaultConnection", connectionString);
            //AddOrUpdateAppSetting("Oqtane:DefaultAlias", connectionString);
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
                // continue with the procress, moving down the tree
                var nextSection = remainingSections[1];
                SetValueRecursively(nextSection, jsonObj[currentSection], value);
            }
            else
            {
                // we've got to the end of the tree, set the value
                jsonObj[currentSection] = value;
            }
        }

        public void StartupMigration()
        {
            var defaultConnectionString = _config.GetConnectionString("DefaultConnection");
            var defaultAlias = _config.GetSection("Oqtane").GetValue("DefaultAlias", string.Empty);
            
            // if no values specified, fallback to IDE installer
            if (string.IsNullOrEmpty(defaultConnectionString) || string.IsNullOrEmpty(defaultAlias))
            {
                IsInstalled = false;
                return;
            }

            var result = MasterMigration(defaultConnectionString, defaultAlias, null, true);
            IsInstalled = result.Success;
            if (_isInstalled)
                BuildDefaultSite();
        }

        public void BuildDefaultSite()
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

                var site = new Site
                {
                    TenantId = -1,
                    Name = "Default Site",
                    LogoFileId = null,
                    DefaultThemeType = Constants.DefaultTheme,
                    DefaultLayoutType = Constants.DefaultLayout,
                    DefaultContainerType = Constants.DefaultContainer,
                };
                site = siteRepository.AddSite(site);

                var user = new User
                {
                    SiteId = site.SiteId,
                    Username = Constants.HostUser,
                    //TODO Decide default password or throw exception ??
                    Password = _config.GetSection("Oqtane").GetValue("DefaultPassword", "oQtane123"),
                    Email = _config.GetSection("Oqtane").GetValue("DefaultEmail", "nobody@cortonso.com"),
                    DisplayName = Constants.HostUser,
                };
                CreateHostUser(folders, userRoles, roles, users, identityUserManager, user);
            }
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
                var folder = folderRepository.GetFolder(user.SiteId, "Users\\");
                if (folder != null)
                    folderRepository.AddFolder(new Folder
                    {
                        SiteId = folder.SiteId, ParentId = folder.FolderId, Name = "My Folder", Path = folder.Path + newUser.UserId + "\\", Order = 1, IsSystem = true,
                        Permissions = "[{\"PermissionName\":\"Browse\",\"Permissions\":\"[" + newUser.UserId + "]\"},{\"PermissionName\":\"View\",\"Permissions\":\"All Users\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"[" +
                                      newUser.UserId + "]\"}]",
                    });
            }
        }
    }
}
