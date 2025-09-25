using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Text.Json;
using System.Net;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata;
using Oqtane.Security;
using System.Security.Policy;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class ThemeController : Controller
    {
        private readonly IThemeRepository _themes;
        private readonly IInstallationManager _installationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantManager _tenantManager;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;
        private readonly IServiceProvider _serviceProvider;

        public ThemeController(IThemeRepository themes, IInstallationManager installationManager, IWebHostEnvironment environment, ITenantManager tenantManager, IUserPermissions userPermissions, ISyncManager syncManager, ILogManager logger, IServiceProvider serviceProvider)
        {
            _themes = themes;
            _installationManager = installationManager;
            _environment = environment;
            _tenantManager = tenantManager;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
            _serviceProvider = serviceProvider;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<Theme> Get(string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                List<Theme> themes = new List<Theme>();
                foreach (Theme theme in _themes.GetThemes(SiteId))
                {
                    if (_userPermissions.IsAuthorized(User, PermissionNames.Utilize, theme.PermissionList))
                    {
                        themes.Add(theme);
                    }
                }
                return themes;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Theme Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                return null;
            }
}

        // GET api/<controller>/5?siteid=x
        [HttpGet("{id}")]
        public Theme Get(int id, string siteid)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                Theme theme = _themes.GetTheme(id, SiteId);
                if (theme != null && _userPermissions.IsAuthorized(User, PermissionNames.Utilize, theme.PermissionList))
                {
                    return theme;
                }
                else
                {
                    if (theme != null)
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Theme Get Attempt {ThemeId} {SiteId}", id, siteid);
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    }
                    else
                    {
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Theme Get Attempt {ThemeId} {SiteId}", id, siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Put(int id, [FromBody] Theme theme)
        {
            if (ModelState.IsValid && theme.SiteId == _alias.SiteId && theme.ThemeId == id && _themes.GetTheme(theme.ThemeId,theme.SiteId) != null)
            {
                _themes.UpdateTheme(theme);
                _syncManager.AddSyncEvent(_alias, EntityNames.Theme, theme.ThemeId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Theme Updated {Theme}", theme);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Theme Put Attempt {Theme}", theme);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // DELETE api/<controller>/5?siteid=x
        [HttpDelete("{themename}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id, int siteid)
        {
            Theme theme = _themes.GetTheme(id, siteid);
            if (theme != null && theme.SiteId == _alias.SiteId && Utilities.GetAssemblyName(theme.ThemeName) != Constants.ClientId)
            {
                // remove theme assets
                if (_installationManager.UninstallPackage(theme.PackageName))
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Theme Assets Removed For {ThemeName}", theme.ThemeName);
                }
                else
                {
                    // attempt to delete assemblies based on naming convention
                    foreach (string asset in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Utilities.GetTypeName(theme.ThemeName) + "*.*"))
                    {
                        System.IO.File.Delete(asset);
                    }
                    _logger.Log(LogLevel.Warning, this, LogFunction.Delete, "Theme Assets Removed For {ThemeName}. Please Note That Some Assets May Have Been Missed Due To A Missing Asset Manifest. An Asset Manifest Is Only Created If A Theme Is Installed From A Nuget Package.", theme.ThemeName);
                }

                // clean up theme static resource folder
                string assetpath = Path.Combine(_environment.WebRootPath, "Themes", Utilities.GetTypeName(theme.ThemeName));
                if (Directory.Exists(assetpath))
                {
                    Directory.Delete(assetpath, true);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Theme Static Resource Folder Removed For {ThemeName}", theme.ThemeName);
                }

                // remove theme
                _themes.DeleteTheme(theme.ThemeId);
                _syncManager.AddSyncEvent(_alias, EntityNames.Theme, theme.ThemeId, SyncEventActions.Delete);
                _syncManager.AddSyncEvent(_alias, EntityNames.Site, theme.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Theme Removed For {ThemeName}", theme.ThemeName);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Theme Delete Attempt {ThemeId} {SiteId}", id, siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET: api/<controller>/templates
        [HttpGet("templates")]
        [Authorize(Roles = RoleNames.Host)]
        public List<Template> GetTemplates()
        {
            var templates = new List<Template>();
            var root = Directory.GetParent(_environment.ContentRootPath);
            string templatePath = Utilities.PathCombine(_environment.WebRootPath, "Themes", "Templates", Path.DirectorySeparatorChar.ToString());
            if (Directory.Exists(templatePath))
            {
                foreach (string directory in Directory.GetDirectories(templatePath))
                {
                    string name = directory.Replace(templatePath, "");
                    if (System.IO.File.Exists(Path.Combine(directory, "template.json")))
                    {
                        var template = JsonSerializer.Deserialize<Template>(System.IO.File.ReadAllText(Path.Combine(directory, "template.json")));
                        template.Name = name;
                        template.Location = "";
                        if (template.Type.ToLower() != "internal")
                        {
                            template.Location = Utilities.PathCombine(root.Parent.ToString(), Path.DirectorySeparatorChar.ToString());
                        }
                        templates.Add(template);
                    }
                    else
                    {
                        templates.Add(new Template { Name = name, Title = name, Type = "External", Version = "", Location = Utilities.PathCombine(root.Parent.ToString(), Path.DirectorySeparatorChar.ToString()) });
                    }
                }
            }
            return templates;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public Theme Post([FromBody] Theme theme)
        {
            if (ModelState.IsValid)
            {
                string rootPath;
                DirectoryInfo rootFolder = Directory.GetParent(_environment.ContentRootPath);
                string templatePath = Utilities.PathCombine(_environment.WebRootPath, "Themes", "Templates", theme.Template, Path.DirectorySeparatorChar.ToString());

                if (!string.IsNullOrEmpty(theme.ThemeName))
                {
                    theme.ThemeName = theme.ThemeName.Replace("[Owner]", theme.Owner).Replace("[Theme]", theme.Name);
                }
                else
                {
                    theme.ThemeName = theme.Owner + ".Theme." + theme.Name;
                }

                if (theme.Template.ToLower().Contains("internal"))
                {
                    rootPath = Utilities.PathCombine(rootFolder.FullName, Path.DirectorySeparatorChar.ToString());
                    theme.ThemeName = theme.ThemeName + ", Oqtane.Client";
                }
                else
                {
                    rootPath = Utilities.PathCombine(rootFolder.Parent.FullName, theme.Owner + ".Theme." + theme.Name, Path.DirectorySeparatorChar.ToString());
                    theme.ThemeName = theme.ThemeName + ", " + theme.ThemeName + ".Client.Oqtane";
                }

                ProcessTemplatesRecursively(new DirectoryInfo(templatePath), rootPath, rootFolder.Name, templatePath, theme);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Theme Created {Theme}", theme);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Theme Post Attempt {Theme}", theme);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                theme = null;
            }

            return theme;
        }

        private void ProcessTemplatesRecursively(DirectoryInfo current, string rootPath, string rootFolder, string templatePath, Theme theme)
        {
            var tokenReplace = InitializeTokenReplace(rootPath, rootFolder, theme);

            // process folder
            var folderPath = Utilities.PathCombine(rootPath, current.FullName.Replace(templatePath, ""));
            folderPath = tokenReplace.ReplaceTokens(folderPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            tokenReplace.AddSource("Folder", folderPath);
            var files = current.GetFiles("*.*");
            if (files != null)
            {
                foreach (FileInfo file in files)
                {
                    // process file
                    var filePath = Path.Combine(folderPath, file.Name);
                    filePath = tokenReplace.ReplaceTokens(filePath);
                    tokenReplace.AddSource("File", Path.GetFileName(filePath));

                    var text = System.IO.File.ReadAllText(file.FullName);
                    text = tokenReplace.ReplaceTokens(text);
                    System.IO.File.WriteAllText(filePath, text);
                }

                var folders = current.GetDirectories();
                foreach (DirectoryInfo folder in folders.Reverse())
                {
                    ProcessTemplatesRecursively(folder, rootPath, rootFolder, templatePath, theme);
                }
            }
        }

        private ITokenReplace InitializeTokenReplace(string rootPath, string rootFolder, Theme theme)
        {
            var tokenReplace = _serviceProvider.GetService<ITokenReplace>();
            tokenReplace.AddSource(() =>
            {
                return new Dictionary<string, object>
                {
                    { "RootPath", rootPath },
                    { "RootFolder", rootFolder },
                    { "Owner", theme.Owner },
                    { "Theme", theme.Name }
                };
            });

            if (theme.Version == "local")
            {
                tokenReplace.AddSource(() =>
                {
                    return new Dictionary<string, object>()
                            {
                                { "FrameworkVersion", Constants.Version },
                                { "ClientReference", $"<Reference Include=\"Oqtane.Client\"><HintPath>..\\..\\{rootFolder}\\Oqtane.Server\\bin\\Debug\\net9.0\\Oqtane.Client.dll</HintPath></Reference>" },
                                { "SharedReference", $"<Reference Include=\"Oqtane.Shared\"><HintPath>..\\..\\{rootFolder}\\Oqtane.Server\\bin\\Debug\\net9.0\\Oqtane.Shared.dll</HintPath></Reference>" },
                            };
                });
            }
            else
            {
                tokenReplace.AddSource(() =>
                {
                    return new Dictionary<string, object>()
                            {
                                { "FrameworkVersion", theme.Version },
                                { "ClientReference", $"<PackageReference Include=\"Oqtane.Client\" Version=\"{theme.Version}\" />" },
                                { "SharedReference", $"<PackageReference Include=\"Oqtane.Shared\" Version=\"{theme.Version}\" />" },
                            };
                });
            }

            return tokenReplace;
        }
    }
}
