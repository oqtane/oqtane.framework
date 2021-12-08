using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Oqtane.Modules;
using Oqtane.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace [Owner].[Module]
{ 
    public partial class Settings: ModuleBase
    {
        string _value;

        [Inject]
        public ISettingService SettingService { get; set; }
        [Inject]
        public IStringLocalizer<Settings> Localizer{ get; set; }

        public override string Title => "[Module] Settings";


        protected override async Task OnInitializedAsync()
        {
            try
            {
                Dictionary<string, string> settings = await SettingService.GetModuleSettingsAsync(ModuleState.ModuleId);
                _value = SettingService.GetSetting(settings, "SettingName", "");
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }
        }

        public async Task UpdateSettings()
        {
            try
            {
                Dictionary<string, string> settings = await SettingService.GetModuleSettingsAsync(ModuleState.ModuleId);
                SettingService.SetSetting(settings, "SettingName", _value);
                await SettingService.UpdateModuleSettingsAsync(settings, ModuleState.ModuleId);
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }
        }
    }
}