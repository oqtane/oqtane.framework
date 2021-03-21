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
    public class HtmlTextManager : IMigratable, IPortable
    {
        private readonly IHtmlTextRepository _htmlText;
        private readonly ISqlRepository _sql;

        public HtmlTextManager(IHtmlTextRepository htmlText, ISqlRepository sql)
        {
            _htmlText = htmlText;
            _sql = sql;
        }

        public bool Migrate(Tenant tenant, MigrationType migrationType)
        {
            var result = true;

            var dbConfig = new DbConfig(null, null) {ConnectionString = tenant.DBConnectionString};
            using (var db = new HtmlTextContext(dbConfig, null))
            {
                try
                {
                    var migrator = db.GetService<IMigrator>();
                    if (migrationType == MigrationType.Down)
                    {
                        migrator.Migrate(Migration.InitialDatabase);
                    }
                    else
                    {
                        migrator.Migrate();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    result = false;
                }

            }
            return result;
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
    }
}
