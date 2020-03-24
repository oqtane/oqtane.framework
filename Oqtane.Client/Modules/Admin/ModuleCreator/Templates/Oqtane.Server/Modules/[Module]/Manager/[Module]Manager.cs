using Oqtane.Models.[Module]s;
using Oqtane.Repository.[Module]s;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Oqtane.Modules.[Module]s
{
    public class [Module]Manager : IPortable
    {
        private I[Module]Repository _[Module]s;

        public [Module]Manager(I[Module]Repository [Module]s)
        {
            _[Module]s = [Module]s;
        }

        public string ExportModule(Models.Module module)
        {
            string content = "";
            List<[Module]> [Module]s = _[Module]s.Get[Module]s(module.ModuleId).ToList();
            if ([Module]s != null)
            {
                content = JsonSerializer.Serialize([Module]s);
            }
            return content;
        }

        public void ImportModule(Models.Module module, string content, string version)
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