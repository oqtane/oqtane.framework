using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    [SiteUpgrade("localhost:44357", "01.00.00")]
    public class ExampleUpgrade : ISiteUpgrade
    {
        bool ISiteUpgrade.Upgrade(Site site, Alias alias)
        {
            // execute some version-specific upgrade logic for the site here such as adding pages, modules, content, etc...
            return true;
        }
    }
}
