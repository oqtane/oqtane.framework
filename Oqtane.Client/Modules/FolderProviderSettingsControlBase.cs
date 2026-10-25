using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Providers;
using Oqtane.Shared;

namespace Oqtane.Modules
{
    [OqtaneIgnore]
    public abstract class FolderProviderSettingsControlBase : ModuleBase, IFolderProviderSettingsControl
    {
        [Parameter]
        public int FolderConfigId { get; set; }

        public abstract Task<IDictionary<string, string>> GetSettingsAsync();
    }
}
