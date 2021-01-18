using Microsoft.AspNetCore.Components;

using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;

using [Owner].[Module].Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace [Owner].[Module]
{
    public partial class Index : ModuleBase
    {
        [Inject()]
        public I[Module]Service [Module]Service { get; set; }

        [Inject()]
        public NavigationManager NavigationManager { get; set; }

        List<Models.[Module]> _[Module]s;

        public override List<Resource> Resources => new List<Resource>()
        {
            new Resource { ResourceType = ResourceType.Stylesheet, Url = ModulePath() + "Module.css" },
            new Resource { ResourceType = ResourceType.Script, Url = ModulePath() + "Module.js" }
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _[Module]s = await [Module]Service.Get[Module]sAsync(ModuleState.ModuleId);
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading [Module] {Error}", ex.Message);
                AddModuleMessage("Error Loading [Module]", MessageType.Error);
            }
        }

        private async Task Delete(Models.[Module] [Module])
        {
            try
            {
                await [Module]Service.Delete[Module]Async([Module].[Module]Id, ModuleState.ModuleId);
                await logger.LogInformation("[Module] Deleted {[Module]}", [Module]);
                _[Module]s = await [Module]Service.Get[Module]sAsync(ModuleState.ModuleId);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Deleting [Module] {[Module]} {Error}", [Module], ex.Message);
                AddModuleMessage("Error Deleting [Module]", MessageType.Error);
            }
        }
    }
}
