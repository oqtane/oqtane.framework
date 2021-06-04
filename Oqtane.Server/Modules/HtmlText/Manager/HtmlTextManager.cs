using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules.HtmlText.Repository;
using System.Net;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Repository;
using Oqtane.Shared;
using Oqtane.Migrations.Framework;

// ReSharper disable ConvertToUsingDeclaration

namespace Oqtane.Modules.HtmlText.Manager
{
    public class HtmlTextManager : MigratableModuleBase, IInstallable, IPortable
    {
        private readonly IHtmlTextRepository _htmlText;
        private readonly ITenantManager _tenantManager;
        private readonly IHttpContextAccessor _accessor;
        private readonly ISqlRepository _sqlRepository;

        public HtmlTextManager(IHtmlTextRepository htmlText, ITenantManager tenantManager, IHttpContextAccessor httpContextAccessor, ISqlRepository sqlRepository)
        {
            _htmlText = htmlText;
            _tenantManager = tenantManager;
            _accessor = httpContextAccessor;
            _sqlRepository = sqlRepository;
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
            if (tenant.DBType == Constants.DefaultDBType && version == "1.0.1")
            {
                // version 1.0.0 used SQL scripts rather than migrations, so we need to seed the migration history table
                _sqlRepository.ExecuteNonQuery(tenant, MigrationUtils.BuildInsertScript("HtmlText.01.00.00.00"));
            }
            return Migrate(new HtmlTextContext(_tenantManager, _accessor), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new HtmlTextContext(_tenantManager, _accessor), tenant, MigrationType.Down);
        }
    }
}
