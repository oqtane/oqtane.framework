using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteTemplateRepository
    {
        IEnumerable<SiteTemplate> GetSiteTemplates();
    }
}
