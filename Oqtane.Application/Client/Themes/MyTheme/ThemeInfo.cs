using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Themes;
using Oqtane.Shared;

namespace Oqtane.Application.MyTheme
{
    public class ThemeInfo : ITheme
    {
        public Oqtane.Models.Theme Theme => new Oqtane.Models.Theme
        {
            Name = "MyTheme",
            Version = "1.0.0",
            PackageName = "Oqtane.Application",
            ThemeSettingsType = "Oqtane.Application.MyTheme.ThemeSettings, Oqtane.Application.Client.Oqtane",
            ContainerSettingsType = "Oqtane.Application.MyTheme.ContainerSettings, Oqtane.Application.Client.Oqtane",
            Resources = new List<Resource>()
            {
                new Stylesheet(Constants.BootstrapStylesheetUrl, Constants.BootstrapStylesheetIntegrity, "anonymous"),
                new Stylesheet("~/Theme.css"),
                new Script(Constants.BootstrapScriptUrl, Constants.BootstrapScriptIntegrity, "anonymous")
            }
        };
    }
}
