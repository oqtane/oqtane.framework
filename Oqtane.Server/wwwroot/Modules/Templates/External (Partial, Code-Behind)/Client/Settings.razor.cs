using Microsoft.AspNetCore.Components;

using Oqtane.Modules;
using Oqtane.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace [Owner].[Module]
{
    public partial class Settings : ModuleBase
    {
        [Inject()]
        public ISettingService SettingService { get; set; }

        private string _value;

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
