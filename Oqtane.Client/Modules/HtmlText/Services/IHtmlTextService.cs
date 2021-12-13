using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Services
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextService 
    {
        Task<Models.HtmlText> GetHtmlTextAsync(int ModuleId);

        Task AddHtmlTextAsync(Models.HtmlText htmltext);

        Task UpdateHtmlTextAsync(Models.HtmlText htmltext);

        Task DeleteHtmlTextAsync(int ModuleId);
    }
}
