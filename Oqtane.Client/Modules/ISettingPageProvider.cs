using System;

namespace Oqtane.Modules
{
    public interface ISettingPageProvider
    {
        Type SettingsPage { get; set; }
    }
}
