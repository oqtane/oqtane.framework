using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Themes;
using System.Collections.Generic;

namespace [Owner].[Theme]
{
    public partial class Theme1 : ThemeBase
    {
        public override string Name => "Theme1";

        public override string Panes => PaneNames.Admin + ",Top Full Width,Top 100%,Left 50%,Right 50%,Left 33%,Center 33%,Right 33%,Left Outer 25%,Left Inner 25%,Right Inner 25%,Right Outer 25%,Left 25%,Center 50%,Right 25%,Left Sidebar 66%,Right Sidebar 33%,Left Sidebar 33%,Right Sidebar 66%,Bottom 100%,Bottom Full Width";

        public override List<Resource> Resources => new List<Resource>()
        {
            new Resource { ResourceType = ResourceType.Stylesheet, Url = "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css", Integrity = "sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC", CrossOrigin = "anonymous" },
            new Resource { ResourceType = ResourceType.Stylesheet, Url = ThemePath() + "Theme.css" },
            new Resource { ResourceType = ResourceType.Script, Bundle = "Bootstrap", Url = "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js", Integrity = "sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM", CrossOrigin = "anonymous" }
        };
    }
}
