using System;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    [SiteMigration("localhost:44357", "01.00.00")]
    public class ExampleSiteMigration : ISiteMigration
    {
        private readonly IServiceProvider _provider;

        public ExampleSiteMigration(IServiceProvider provider)
        {
            _provider = provider;
        }

        void ISiteMigration.Up(Site site, Alias alias)
        {
            // execute some version-specific upgrade logic for the site here such as adding pages, modules, content, etc...
            // note that you need to use IServiceProvider to resolve any services you need for your migrations
        }

        void ISiteMigration.Down(Site site, Alias alias)
        {
            // not implemented
        }
    }
}
