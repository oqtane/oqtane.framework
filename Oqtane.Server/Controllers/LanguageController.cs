using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class LanguageController : Controller
    {
        private readonly ILanguageRepository _languages;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public LanguageController(ILanguageRepository languages, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _languages = languages;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        [HttpGet]
        public IEnumerable<Language> Get(string siteid, string packagename)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && (SiteId == _alias.SiteId || SiteId == -1))
            {
                List<Language> languages = new List<Language>();
                if (SiteId == -1)
                {
                    if (!string.IsNullOrEmpty(packagename))
                    {
                        foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{packagename}*{Constants.SatelliteAssemblyExtension}", SearchOption.AllDirectories))
                        {
                            var code = Path.GetFileName(Path.GetDirectoryName(file));
                            if (!languages.Any(item => item.Code == code))
                            {
                                languages.Add(new Language { Code = code, Name = CultureInfo.GetCultureInfo(code).DisplayName, Version = FileVersionInfo.GetVersionInfo(file).ProductVersion, IsDefault = false });
                            }
                        }
                    }
                }
                else
                {
                    languages = _languages.GetLanguages(SiteId).ToList();
                    if (!string.IsNullOrEmpty(packagename))
                    {
                        foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{packagename}*{Constants.SatelliteAssemblyExtension}", SearchOption.AllDirectories))
                        {
                            var code = Path.GetFileName(Path.GetDirectoryName(file));
                            if (languages.Any(item => item.Code == code))
                            {
                                languages.Single(item => item.Code == code).Version = FileVersionInfo.GetVersionInfo(file).ProductVersion;
                            }
                        }
                    }
                    var defaultCulture = CultureInfo.GetCultureInfo(Constants.DefaultCulture);
                    languages.Add(new Language { Code = defaultCulture.Name, Name = defaultCulture.DisplayName, Version = Constants.Version, IsDefault = !languages.Any(l => l.IsDefault) });
                }
                return languages.OrderBy(item => item.Name);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Language Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        [HttpGet("{id}")]
        public Language Get(int id)
        {
            var language = _languages.GetLanguage(id);
            if (language != null && language.SiteId == _alias.SiteId)
            {
                return language;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Language Get Attempt {LanguageId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public Language Post([FromBody] Language language)
        {
            if (ModelState.IsValid && language.SiteId == _alias.SiteId)
            {
                language = _languages.AddLanguage(language);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Language, language.LanguageId, SyncEventActions.Create);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Language Added {Language}", language);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Language Post Attempt {Language}", language);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                language = null;
            }
            return language;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Delete(int id)
        {
            var language = _languages.GetLanguage(id);
            if (language != null && language.SiteId == _alias.SiteId)
            {
                _languages.DeleteLanguage(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Language, language.LanguageId, SyncEventActions.Delete);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Language Deleted {LanguageId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Language Delete Attempt {LanguageId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
