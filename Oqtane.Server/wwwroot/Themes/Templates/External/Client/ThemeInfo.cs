using Oqtane.Models;
using Oqtane.Themes;

namespace [Owner].[Theme]
{
    public class ThemeInfo : ITheme
    {
        public Theme Theme => new Theme
        {
            Name = "[Theme]",
            Version = "1.0.0",
            PackageName = "[Owner].[Theme]",
            Resources = new List<Resource>()
            {
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.2.0/css/bootstrap.min.css", Integrity = "sha512-XWTTruHZEYJsxV3W/lSXG1n3Q39YIWOstqvmFsdNEEQfHoZ6vm6E9GK2OrF6DSJSpIbRbi+Nn0WDPID9O7xB2Q==", CrossOrigin = "anonymous" },
                new Resource { ResourceType = ResourceType.Stylesheet, Url = "~/Theme.css" },
                new Resource { ResourceType = ResourceType.Script, Url = "https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.2.0/js/bootstrap.bundle.min.js", Integrity = "sha512-9GacT4119eY3AcosfWtHMsT5JyZudrexyEVzTBWV3viP/YfB9e2pEy3N7WXL3SV6ASXpTU0vzzSxsbfsuUH4sQ==", CrossOrigin = "anonymous" }
            }
        };

    }
}
