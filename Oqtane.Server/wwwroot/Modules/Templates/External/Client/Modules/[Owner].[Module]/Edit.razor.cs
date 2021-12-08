using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using [Owner].[Module].Services;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace [Owner].[Module]
{
    [inject] public  I[Module]Service [Module]Service { get; set; }
    [inject] public  NavigationManager NavigationManager { get; set; }
    [inject] public IStringLocalizer<Edit> Localizer { get; set; }
    public partial class Edit: ModuleBase
    {
    public override SecurityAccessLevel SecurityAccessLevel => SecurityAccessLevel.Edit;

    public override string Actions => "Add,Edit";

    public override string Title => "Manage [Module]";

    public override List<Resource> Resources => new List<Resource>()
    {
        new Resource { ResourceType = ResourceType.Stylesheet, Url = ModulePath() + "Module.css" }
    };

    private ElementReference form;
    private bool validated = false;

    private int _id;
    private string _name;
    private string _createdby;
    private DateTime _createdon;
    private string _modifiedby;
    private DateTime _modifiedon;
   
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
            AddModuleMessage(Localizer["Message.LoadError"], MessageType.Error);
        }
    }

    private async Task Save()
    {
        try
        {
            validated = true;
            var interop = new Oqtane.UI.Interop(JSRuntime);
            if (await interop.FormValid(form))
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
            else
            {
                AddModuleMessage(Localizer["Message.SaveValidation"], MessageType.Warning);
            }
        }
        catch (Exception ex)
        {
            await logger.LogError(ex, "Error Saving [Module] {Error}", ex.Message);
            AddModuleMessage(Localizer["Message.SaveError"], MessageType.Error);
        }
    }
}
