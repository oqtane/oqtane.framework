using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using [Owner].[Module].Services;

namespace [Owner].[Module]
{
    public partial class Index : ModuleBase
    {
        List<Models.[Module]> _[Module]s;
        [Inject] public I[Module]Service [Module]Service { get; set; }
        [Inject] public  NavigationManager NavigationManager { get; set; }
        [Inject] public  IStringLocalizer<Index> Localizer { get; set; }

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
                AddModuleMessage(Localizer["Message.LoadError"], MessageType.Error);
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
                AddModuleMessage(Localizer["Message.DeleteError"], MessageType.Error);
            }
       }
    }
}