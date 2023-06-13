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
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootswatch/5.3.0/cyborg/bootstrap.min.css", Integrity = "sha512-jwIqEv8o/kTBMJVtbNCBrDqhBojl0YSUam+EFpLjVOC86Ci6t4ZciTnIkelFNOik+dEQVymKGcQLiaJZNAfWRg==", CrossOrigin = "anonymous" },
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "~/Theme.css" },
                new Resource { ResourceType = ResourceType.Script, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.0/js/bootstrap.bundle.min.js", Integrity = "sha512-VK2zcvntEufaimc+efOYi622VN5ZacdnufnmX7zIhCPmjhKnOi9ZDMtg1/ug5l183f19gG1/cBstPO4D8N/Img==", CrossOrigin = "anonymous" }
            }
        };
    }
}
