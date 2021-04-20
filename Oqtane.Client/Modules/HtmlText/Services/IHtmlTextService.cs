using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Services
{
    public interface IHtmlTextService 
    {
        Task<Models.HtmlText> GetHtmlTextAsync(int ModuleId);

        Task AddHtmlTextAsync(Models.HtmlText htmltext);

        Task UpdateHtmlTextAsync(Models.HtmlText htmltext);

        Task DeleteHtmlTextAsync(int ModuleId);
    }
}
