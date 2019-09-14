using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Services
{
    public interface IHtmlTextService 
    {
        Task<HtmlTextInfo> GetHtmlTextAsync(int ModuleId);

        Task AddHtmlTextAsync(HtmlTextInfo htmltext);

        Task UpdateHtmlTextAsync(HtmlTextInfo htmltext);

        Task DeleteHtmlTextAsync(int ModuleId);
    }
}
