using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Themes.OqtaneTheme
{
    [PrivateApi("Mark Build-In Theme-Info classes as private, since it's not very useful in the public docs")]
    public class ThemeInfo : ITheme
    {
        public Theme Theme => new Theme
        {
            Name = "Oqtane Theme",
            Version = "1.0.0",
            ThemeSettingsType = "Oqtane.Themes.OqtaneTheme.ThemeSettings, Oqtane.Client",
            ContainerSettingsType = "Oqtane.Themes.OqtaneTheme.ContainerSettings, Oqtane.Client",
            Resources = new List<Resource>()
            {
		        // obtained from https://cdnjs.com/libraries/bootswatch
                new Stylesheet("https://cdnjs.cloudflare.com/ajax/libs/bootswatch/5.3.7/cyborg/bootstrap.min.css", "sha512-/LQFzDeXqysGQ/POl5YOEjgVZH1BmqDHvshhnFIChf50bMGQ470qhUrsecD9MRCUwzwqRoshwAbmA2oTW4I6Yg==", "anonymous"),
                new Stylesheet("~/Theme.css"),
                new Script(Constants.BootstrapScriptUrl, Constants.BootstrapScriptIntegrity, "anonymous")
            }
        };
    }
}
