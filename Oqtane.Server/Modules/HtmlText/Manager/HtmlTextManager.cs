using System;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using System.Net;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Enums;
// ReSharper disable ConvertToUsingDeclaration

namespace Oqtane.Modules.HtmlText.Manager
{
    public class HtmlTextManager : MigratableModuleBase, IInstallable, IPortable
    {
        private readonly IHtmlTextRepository _htmlText;
        private readonly ISqlRepository _sql;

        public HtmlTextManager(IHtmlTextRepository htmlText, ISqlRepository sql)
        {
            _htmlText = htmlText;
            _sql = sql;
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
                htmlText = new HtmlTextInfo();
                htmlText.ModuleId = module.ModuleId;
                htmlText.Content = content;
                _htmlText.AddHtmlText(htmlText);
            }
        }

        public bool Install(Tenant tenant, string version)
        {
            var dbConfig = new DbConfig(null, null) {ConnectionString = tenant.DBConnectionString, DatabaseType = tenant.DBType};
            return Migrate(new HtmlTextContext(dbConfig, null), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            var dbConfig = new DbConfig(null, null) {ConnectionString = tenant.DBConnectionString, DatabaseType = tenant.DBType};
            return Migrate(new HtmlTextContext(dbConfig, null), tenant, MigrationType.Down);
        }
    }
}
