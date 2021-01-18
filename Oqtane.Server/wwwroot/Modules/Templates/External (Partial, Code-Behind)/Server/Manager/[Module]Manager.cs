using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using [Owner].[Module].Models;
using [Owner].[Module].Repository;

namespace [Owner].[Module].Manager
{
    public class [Module]Manager : IInstallable, IPortable
    {
        private I[Module]Repository _[Module]Repository;
        private ISqlRepository _sql;

        public [Module]Manager(I[Module]Repository [Module]Repository, ISqlRepository sql)
        {
            _[Module]Repository = [Module]Repository;
            _sql = sql;
        }

        public bool Install(Tenant tenant, string version)
        {
            return _sql.ExecuteScript(tenant, GetType().Assembly, "[Owner].[Module]." + version + ".sql");
        }

        public bool Uninstall(Tenant tenant)
        {
            return _sql.ExecuteScript(tenant, GetType().Assembly, "[Owner].[Module].Uninstall.sql");
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