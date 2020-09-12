using Oqtane.Models;
using Oqtane.Shared;
using System.Collections.Generic;

namespace Oqtane.Themes.BlazorTheme
{
    public partial class Default : ThemeBase
    {
        public override string Panes => "Content";

        public override List<Resource> Resources => new List<Resource>()
        {
            new Resource { ResourceType = ResourceType.Stylesheet, Url = "https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css", Integrity = "sha384-9aIt2nRpC12Uk9gS9baDl411NQApFmC26EwAOH8WgZl5MYYxFfc+NcPb1dKGj7Sk", CrossOrigin = "anonymous" },
            new Resource { ResourceType = ResourceType.Stylesheet, Url = ThemePath() + "Theme.css" },
            new Resource { ResourceType = ResourceType.Script, Bundle = "Bootstrap", Url = "https://code.jquery.com/jquery-3.5.1.slim.min.js", Integrity = "sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj", CrossOrigin = "anonymous" },
            new Resource { ResourceType = ResourceType.Script, Bundle = "Bootstrap", Url = "https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js", Integrity = "sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo", CrossOrigin = "anonymous" },
            new Resource { ResourceType = ResourceType.Script, Bundle = "Bootstrap", Url = "https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js", Integrity = "sha384-OgVRvuATP1z7JjHLkuOU7Xw704+h835Lr+6QL9UvYjZE3Ipu6Tp75j7Bh/kR0JKI", CrossOrigin = "anonymous" }
        };
    }
}
