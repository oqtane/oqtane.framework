using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Themes;
using Oqtane.Shared;

namespace [Owner].Theme.[Theme]
{
    public class ThemeInfo : ITheme
    {
        public Oqtane.Models.Theme Theme => new Oqtane.Models.Theme
        {
            Name = "[Owner] [Theme]",
            Version = "1.0.0",
            PackageName = "[Owner].Theme.[Theme]",
            ThemeSettingsType = "[Owner].Theme.[Theme].ThemeSettings, [Owner].Theme.[Theme].Client.Oqtane",
            ContainerSettingsType = "[Owner].Theme.[Theme].ContainerSettings, [Owner].Theme.[Theme].Client.Oqtane",
            Resources = new List<Resource>()
            {
		        // obtained from https://cdnjs.com/libraries
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/css/bootstrap.min.css", Integrity = "sha512-ZuRTqfQ3jNAKvJskDAU/hxbX1w25g41bANOVd1Co6GahIe2XjM6uVZ9dh0Nt3KFCOA061amfF2VeL60aJXdwwQ==", CrossOrigin = "anonymous" },
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "~/Theme.css" },
                new Resource { ResourceType = ResourceType.Script, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/js/bootstrap.bundle.min.js", Integrity = "sha512-WW8/jxkELe2CAiE4LvQfwm1rajOS8PHasCCx+knHG0gBHt8EXxS6T6tJRTGuDQVnluuAvMxWF4j8SNFDKceLFg==", CrossOrigin = "anonymous" }
            }
        };

    }
}
