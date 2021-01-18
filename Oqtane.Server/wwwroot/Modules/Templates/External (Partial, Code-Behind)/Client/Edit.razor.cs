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
    public partial class Edit : ModuleBase
    {
        [Inject()]
        public I[Module]Service [Module]Service { get; set; }

        [Inject()]
        public NavigationManager NavigationManager { get; set; }

        int _id;
        
        string _name;
        
        string _createdby;
        
        DateTime _createdon;
        
        string _modifiedby;

        DateTime _modifiedon;

        public override SecurityAccessLevel SecurityAccessLevel => SecurityAccessLevel.Edit;

        public override string Actions => "Add,Edit";

        public override string Title => "Manage [Module]";

        public override List<Resource> Resources => new List<Resource>()
        {
            new Resource { ResourceType = ResourceType.Stylesheet, Url = ModulePath() + "Module.css" }
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (PageState.Action == "Edit")
                {
                    _id = Int32.Parse(PageState.QueryString["id"]);
                    Models.[Module] [Module] = await [Module]Service.Get[Module]Async(_id, ModuleState.ModuleId);
                    if ([Module] != null)
                    {
                        _name = [Module].Name;
                        _createdby = [Module].CreatedBy;
                        _createdon = [Module].CreatedOn;
                        _modifiedby = [Module].ModifiedBy;
                        _modifiedon = [Module].ModifiedOn;
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading [Module] {[Module]Id} {Error}", _id, ex.Message);
                AddModuleMessage("Error Loading [Module]", MessageType.Error);
            }
        }

        private async Task Save()
        {
            try
            {
                if (PageState.Action == "Add")
                {
                    Models.[Module] [Module] = new Models.[Module]();
                    [Module].ModuleId = ModuleState.ModuleId;
                    [Module].Name = _name;
                    [Module] = await [Module]Service.Add[Module]Async([Module]);
                    await logger.LogInformation("[Module] Added {[Module]}", [Module]);
                }
                else
                {
                    Models.[Module] [Module] = await [Module]Service.Get[Module]Async(_id, ModuleState.ModuleId);
                    [Module].Name = _name;
                    await [Module]Service.Update[Module]Async([Module]);
                    await logger.LogInformation("[Module] Updated {[Module]}", [Module]);
                }
                NavigationManager.NavigateTo(NavigateUrl());
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Saving [Module] {Error}", ex.Message);
                AddModuleMessage("Error Saving [Module]", MessageType.Error);
            }
        }
    }
}
