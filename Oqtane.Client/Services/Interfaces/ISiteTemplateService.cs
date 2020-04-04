using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISiteTemplateService
    {
        Task<List<SiteTemplate>> GetSiteTemplatesAsync();
    }
}
