using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Enums;
using [Owner].[Module].Repository;

namespace [Owner].[Module].Manager
{
    public class [Module]Manager : MigratableModuleBase, IInstallable, IPortable
    {
        private I[Module]Repository _[Module]Repository;
        private readonly ITenantManager _tenantManager;
        private readonly IHttpContextAccessor _accessor;

        public [Module]Manager(I[Module]Repository [Module]Repository, ITenantManager tenantManager, IHttpContextAccessor accessor)
        {
            _[Module]Repository = [Module]Repository;
            _tenantManager = tenantManager;
            _accessor = accessor;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new [Module]Context(_tenantManager, _accessor), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new [Module]Context(_tenantManager, _accessor), tenant, MigrationType.Down);
        }

        public string ExportModule(Module module)
        {
            string content = "";
            List<Models.[Module]> [Module]s = _[Module]Repository.Get[Module]s(module.ModuleId).ToList();
            if ([Module]s != null)
            {
                content = JsonSerializer.Serialize([Module]s);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
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
    }
}