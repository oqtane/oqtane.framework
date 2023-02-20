using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Modules
{
    public interface ISitemap
    {
        // You Must Set The "ServerManagerType" In Your IModule Interface

        List<Sitemap> GetUrls(string alias, string path, Module module);
    }
}
