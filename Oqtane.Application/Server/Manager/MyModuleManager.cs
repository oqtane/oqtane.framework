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
using Oqtane.Application.Repository;
using System.Threading.Tasks;

namespace Oqtane.Application.Manager
{
    public class MyModuleManager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly IMyModuleRepository _MyModuleRepository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public MyModuleManager(IMyModuleRepository MyModuleRepository, IDBContextDependencies DBContextDependencies)
        {
            _MyModuleRepository = MyModuleRepository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new Context(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new Context(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Module module)
        {
            string content = "";
            List<Models.MyModule> MyModules = _MyModuleRepository.GetMyModules(module.ModuleId).ToList();
            if (MyModules != null)
            {
                content = JsonSerializer.Serialize(MyModules);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
        {
            List<Models.MyModule> MyModules = null;
            if (!string.IsNullOrEmpty(content))
            {
                MyModules = JsonSerializer.Deserialize<List<Models.MyModule>>(content);
            }
            if (MyModules != null)
            {
                foreach(var Task in MyModules)
                {
                    _MyModuleRepository.AddMyModule(new Models.MyModule { ModuleId = module.ModuleId, Name = Task.Name });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var MyModule in _MyModuleRepository.GetMyModules(pageModule.ModuleId))
           {
               if (MyModule.ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "MyModule",
                       EntityId = MyModule.MyModuleId.ToString(),
                       Title = MyModule.Name,
                       Body = MyModule.Name,
                       ContentModifiedBy = MyModule.ModifiedBy,
                       ContentModifiedOn = MyModule.ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}
