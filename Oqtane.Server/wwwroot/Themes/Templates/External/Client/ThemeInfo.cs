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
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/css/bootstrap.min.css", Integrity = "sha512-t4GWSVZO1eC8BM339Xd7Uphw5s17a86tIZIj8qRxhnKub6WoyhnrxeCIMeAqBPgdZGlCcG2PrZjMc+Wr78+5Xg==", CrossOrigin = "anonymous" },
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "~/Theme.css" },
                new Resource { ResourceType = ResourceType.Script, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/js/bootstrap.bundle.min.js", Integrity = "sha512-X/YkDZyjTf4wyc2Vy16YGCPHwAY8rZJY+POgokZjQB2mhIRFJCckEGc6YyX9eNsPfn0PzThEuNs+uaomE5CO6A==", CrossOrigin = "anonymous" }
            }
        };

    }
}
