using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using [Owner].[Module]s.Models;
using [Owner].[Module]s.Repository;

namespace [Owner].[Module]s.Manager
{
    public class [Module]Manager : IPortable
    {
        private I[Module]Repository _[Module]s;

        public [Module]Manager(I[Module]Repository [Module]s)
        {
            _[Module]s = [Module]s;
        }

        public string ExportModule(Module module)
        {
            string content = "";
            List<[Module]> [Module]s = _[Module]s.Get[Module]s(module.ModuleId).ToList();
            if ([Module]s != null)
            {
                content = JsonSerializer.Serialize([Module]s);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
        {
            List<[Module]> [Module]s = null;
            if (!string.IsNullOrEmpty(content))
            {
                [Module]s = JsonSerializer.Deserialize<List<[Module]>>(content);
            }
            if ([Module]s != null)
            {
                foreach([Module] [Module] in [Module]s)
                {
                    [Module] _[Module] = new [Module]();
                    _[Module].ModuleId = module.ModuleId;
                    _[Module].Name = [Module].Name;
                    _[Module]s.Add[Module](_[Module]);
                }
            }
        }
    }
}