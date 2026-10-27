using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Providers;
using Oqtane.Shared;

namespace Oqtane.Modules
{
    [OqtaneIgnore]
    public abstract class StorageProviderSettingsControlBase : ModuleBase, IStorageProviderSettingsControl
    {
        [Parameter]
        public int StorageLocationId { get; set; }

        public abstract Task<IDictionary<string, string>> GetSettingsAsync();
    }
}
