using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
{
    public interface ISiteTemplateService
    {
        Task<List<SiteTemplate>> GetSiteTemplatesAsync();
    }
}
