using Oqtane.Models;
using System.Collections.Generic;

namespace Oqtane.Infrastructure.Interfaces
{
    public interface ISiteTemplate
    {
        string Name { get; }

        List<PageTemplate> CreateSite(Site site);
    }
}
