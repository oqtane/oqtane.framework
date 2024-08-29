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
		        // obtained from https://cdnjs.com/libraries
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootswatch/5.3.3/cyborg/bootstrap.min.css",
                    Integrity = "sha512-M+Wrv9LTvQe81gFD2ZE3xxPTN5V2n1iLCXsldIxXvfs6tP+6VihBCwCMBkkjkQUZVmEHBsowb9Vqsq1et1teEg==",
                    CrossOrigin = "anonymous" },
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "~/Theme.css" },
                new Resource { ResourceType = ResourceType.Script, Url = Constants.BootstrapScriptUrl, Integrity = Constants.BootstrapScriptIntegrity, CrossOrigin = "anonymous", Location = ResourceLocation.Body }
            }
        };
    }
}
