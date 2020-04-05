using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Infrastructure
{
    public interface ISiteTemplate
    {
        string Name { get; }

        List<PageTemplate> CreateSite(Site site);
    }
}
