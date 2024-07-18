using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Enums;
using Oqtane.Repository;
using [Owner].Module.[Module].Repository;
using System.Threading.Tasks;

namespace [Owner].Module.[Module].Manager
{
    public class [Module]Manager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly I[Module]Repository _[Module]Repository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public [Module]Manager(I[Module]Repository [Module]Repository, IDBContextDependencies DBContextDependencies)
        {
            _[Module]Repository = [Module]Repository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new [Module]Context(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new [Module]Context(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Oqtane.Models.Module module)
        {
            string content = "";
            List<Models.[Module]> [Module]s = _[Module]Repository.Get[Module]s(module.ModuleId).ToList();
            if ([Module]s != null)
            {
                content = JsonSerializer.Serialize([Module]s);
            }
            return content;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Models.[Module]> [Module]s = null;
            if (!string.IsNullOrEmpty(content))
            {
                [Module]s = JsonSerializer.Deserialize<List<Models.[Module]>>(content);
            }
            if ([Module]s != null)
            {
                foreach(var [Module] in [Module]s)
                {
                    _[Module]Repository.Add[Module](new Models.[Module] { ModuleId = module.ModuleId, Name = [Module].Name });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var [Module] in _[Module]Repository.Get[Module]s(pageModule.ModuleId))
           {
               if ([Module].ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "[Owner][Module]",
                       EntityId = [Module].[Module]Id.ToString(),
                       Title = [Module].Name,
                       Body = [Module].Name,
                       ContentModifiedBy = [Module].ModifiedBy,
                       ContentModifiedOn = [Module].ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}
