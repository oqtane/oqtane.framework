using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository.Interfaces
{
    public interface ISiteTemplateRepository
    {
        IEnumerable<SiteTemplate> GetSiteTemplates();
    }
}
