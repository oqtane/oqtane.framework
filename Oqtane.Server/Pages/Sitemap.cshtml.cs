using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class SitemapModel : PageModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPageRepository _pages;
        private readonly IPageModuleRepository _pageModules;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly ISettingRepository _settings;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public SitemapModel(IServiceProvider serviceProvider, IPageRepository pages, IPageModuleRepository pageModules, IModuleDefinitionRepository moduleDefinitions, ISettingRepository settings, IUserPermissions userPermissions, IUrlMappingRepository urlMappings, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _serviceProvider = serviceProvider;
            _pages = pages;
            _pageModules = pageModules;
            _moduleDefinitions = moduleDefinitions;
            _settings = settings;
            _userPermissions = userPermissions;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        public IActionResult OnGet()
        {
            var sitemap = new List<Sitemap>();

            // build site map
            var rooturl = _alias.Protocol + (string.IsNullOrEmpty(_alias.Path) ? _alias.Name : _alias.Name.Substring(0, _alias.Name.IndexOf("/")));
            var moduleDefinitions = _moduleDefinitions.GetModuleDefinitions(_alias.SiteId).ToList();
            var pageModules = _pageModules.GetPageModules(_alias.SiteId);
            foreach (var page in _pages.GetPages(_alias.SiteId))
            {
                if (!page.IsDeleted && _userPermissions.IsAuthorized(null, PermissionNames.View, page.PermissionList) && !Constants.InternalPagePaths.Contains(page.Path))
                {
                    var pageurl = rooturl;
                    if (string.IsNullOrEmpty(page.Url))
                    {
                        pageurl += Utilities.NavigateUrl(_alias.Path, page.Path, "");
                    }
                    else
                    {
                        pageurl += (page.Url.StartsWith("/") ? "" : "/") + page.Url;
                    }
                    sitemap.Add(new Sitemap { Url = pageurl, ModifiedOn = DateTime.UtcNow });

                    foreach (var pageModule in pageModules.Where(item => item.PageId == page.PageId))
                    {
                        if (_userPermissions.IsAuthorized(null, PermissionNames.View, pageModule.Module.PermissionList))
                        {
                            var moduleDefinition = moduleDefinitions.Where(item => item.ModuleDefinitionName == pageModule.Module.ModuleDefinitionName).FirstOrDefault();
                            if (moduleDefinition != null && moduleDefinition.ServerManagerType != "")
                            {
                                Type moduletype = Type.GetType(moduleDefinition.ServerManagerType);
                                if (moduletype != null && moduletype.GetInterface(nameof(ISitemap)) != null)
                                {
                                    try
                                    {
                                        pageModule.Module.Settings = _settings.GetSettings(EntityNames.Module, pageModule.ModuleId).ToDictionary(x => x.SettingName, x => x.SettingValue);
                                        var moduleobject = ActivatorUtilities.CreateInstance(_serviceProvider, moduletype);
                                        var urls = ((ISitemap)moduleobject).GetUrls(_alias.Path, page.Path, pageModule.Module);
                                        foreach (var url in urls)
                                        {
                                            sitemap.Add(new Sitemap { Url = rooturl + url.Url, ModifiedOn = DateTime.UtcNow });
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Log(LogLevel.Error, this, LogFunction.Other, ex, "Error Retrieving SiteMap For {Name} Module", moduleDefinition.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // write XML
            var builder = new StringBuilder();
            var stringWriter = new StringWriterWithEncoding(builder, Encoding.UTF8);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                CloseOutput = true,
                WriteEndDocumentOnClose = true
            };

            using (var writer = XmlWriter.Create(stringWriter, settings))
            {
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                foreach (var url in sitemap)
                {
                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", url.Url);
                    writer.WriteElementString("lastmod", url.ModifiedOn.ToString("yyyy-MM-dd"));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Close();
            }

            return Content(builder.ToString(), "application/xml");
        }
    }

    public class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding _encoding;

        public StringWriterWithEncoding(StringBuilder builder, Encoding encoding) : base(builder)
        {
            this._encoding = encoding;
        }

        public override Encoding Encoding
        {
            get
            {
                return this._encoding;
            }
        }
    }
}
