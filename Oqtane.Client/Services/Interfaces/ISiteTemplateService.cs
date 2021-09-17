using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retreive <see cref="SiteTemplate"/> entries
    /// </summary>
    public interface ISiteTemplateService
    {
        /// <summary>
        /// Returns a list of site templates
        /// </summary>
        /// <returns></returns>
        Task<List<SiteTemplate>> GetSiteTemplatesAsync();
    }
}
