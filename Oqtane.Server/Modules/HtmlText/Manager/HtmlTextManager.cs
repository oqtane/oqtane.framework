using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Documentation;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Migrations.Framework;
using Oqtane.Models;
using Oqtane.Modules.HtmlText.Repository;
using Oqtane.Repository;
using Oqtane.Shared;

// ReSharper disable ConvertToUsingDeclaration

namespace Oqtane.Modules.HtmlText.Manager
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextManager : MigratableModuleBase, IInstallable, IPortable, ISynchronizable, ISearchable
    {
        private readonly IHtmlTextRepository _htmlTextRepository;
        private readonly IDBContextDependencies _DBContextDependencies;
        private readonly ISqlRepository _sqlRepository;
        private readonly ITenantManager _tenantManager;
        private readonly IMemoryCache _cache;

        public HtmlTextManager(IHtmlTextRepository htmlTextRepository, IDBContextDependencies DBContextDependencies, ISqlRepository sqlRepository, ITenantManager tenantManager, IMemoryCache cache)
        {
            _htmlTextRepository = htmlTextRepository;
            _DBContextDependencies = DBContextDependencies;
            _sqlRepository = sqlRepository;
            _tenantManager = tenantManager;
            _cache = cache;
        }

        // IInstallable implementation
        public bool Install(Tenant tenant, string version)
        {
            if (tenant.DBType == Constants.DefaultDBType && version == "1.0.1")
            {
                // version 1.0.0 used SQL scripts rather than migrations, so we need to seed the migration history table
                _sqlRepository.ExecuteNonQuery(tenant, MigrationUtils.BuildInsertScript("HtmlText.01.00.00.00"));
            }
            return Migrate(new HtmlTextContext(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new HtmlTextContext(_DBContextDependencies), tenant, MigrationType.Down);
        }

        // IPortable implementation
        public string ExportModule(Module module)
        {
            string content = "";
            var htmltext = _htmlTextRepository.GetHtmlText(module.ModuleId);
            if (htmltext != null)
            {
                content = WebUtility.HtmlEncode(htmltext.Content);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
        {
            SaveModuleContent(module, content);
        }

        // ISynchronizable implementation
        public string ExtractModule(Module module, DateTime lastSynchronizedOn)
        {
            string content = "";
            var htmltext = _htmlTextRepository.GetHtmlText(module.ModuleId);
            if (htmltext != null && htmltext.CreatedOn > lastSynchronizedOn)
            {
                content = WebUtility.HtmlEncode(htmltext.Content);
            }
            return content;
        }

        public void LoadModule(Module module, string content)
        {
            SaveModuleContent(module, content);
        }

        private void SaveModuleContent(Module module, string content)
        {
            content = WebUtility.HtmlDecode(content);
            var htmlText = new Models.HtmlText();
            htmlText.ModuleId = module.ModuleId;
            htmlText.Content = content;
            _htmlTextRepository.AddHtmlText(htmlText);

            //clear the cache for the module
            var alias = _tenantManager.GetAlias();
            if (alias != null)
            {
                _cache.Remove($"HtmlText:{alias.SiteKey}:{module.ModuleId}");
            }
        }

        // ISearchable implementation
        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
            var searchContents = new List<SearchContent>();

            var htmltext = _htmlTextRepository.GetHtmlText(pageModule.ModuleId);
            if (htmltext != null && htmltext.CreatedOn >= lastIndexedOn)
            {
                searchContents.Add(new SearchContent
                {
                    Body = htmltext.Content,
                    ContentModifiedBy = htmltext.CreatedBy,
                    ContentModifiedOn = htmltext.CreatedOn
                });
            }

            return Task.FromResult(searchContents);
        }
    }
}
