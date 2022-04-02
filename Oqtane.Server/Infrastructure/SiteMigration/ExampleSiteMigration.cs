using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    [SiteMigration("localhost:44357", "01.00.00")]
    public class ExampleSiteMigration : ISiteMigration
    {
        void ISiteMigration.Up(Site site, Alias alias)
        {
            // execute some version-specific upgrade logic for the site here such as adding pages, modules, content, etc...
        }

        void ISiteMigration.Down(Site site, Alias alias)
        {
            // not implemented
        }
    }
}
