using System;
using System.Collections.Generic;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Enums;
using Oqtane.Interfaces;

// ReSharper disable ConvertToUsingDeclaration

namespace Oqtane.Modules.HtmlText.Manager
{
    public class HtmlTextManager : MigratableModuleBase, IInstallable, IPortable
    {
        private readonly IHtmlTextRepository _htmlText;
        private readonly ITenantManager _tenantManager;
        private readonly IHttpContextAccessor _accessor;


        public HtmlTextManager(IHtmlTextRepository htmlText, ITenantManager tenantManager, IHttpContextAccessor httpContextAccessor)
        {
            _htmlText = htmlText;
            _tenantManager = tenantManager;
            _accessor = httpContextAccessor;
        }

        public string ExportModule(Module module)
        {
            string content = "";
            var htmlText = _htmlText.GetHtmlText(module.ModuleId);
            if (htmlText != null)
            {
                content = WebUtility.HtmlEncode(htmlText.Content);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
        {
            content = WebUtility.HtmlDecode(content);
            var htmlText = _htmlText.GetHtmlText(module.ModuleId);
            if (htmlText != null)
            {
                htmlText.Content = content;
                _htmlText.UpdateHtmlText(htmlText);
            }
            else
            {
                htmlText = new Models.HtmlText();
                htmlText.ModuleId = module.ModuleId;
                htmlText.Content = content;
                _htmlText.AddHtmlText(htmlText);
            }
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new HtmlTextContext(_tenantManager, _accessor), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new HtmlTextContext(_tenantManager, _accessor), tenant, MigrationType.Down);
        }
    }
}
