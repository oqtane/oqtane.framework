using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Shared.Modules.HtmlText.Models;

namespace Oqtane.Client.Modules.HtmlText.Services
{
    public interface IHtmlTextService 
    {
        Task<HtmlTextInfo> GetHtmlTextAsync(int ModuleId);

        Task AddHtmlTextAsync(HtmlTextInfo htmltext);

        Task UpdateHtmlTextAsync(HtmlTextInfo htmltext);

        Task DeleteHtmlTextAsync(int HtmlTextId);
    }
}
