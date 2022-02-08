using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Documentation;

namespace Oqtane.Modules.HtmlText.Services
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextService 
    {
        Task<List<Models.HtmlText>> GetHtmlTextsAsync(int moduleId);

        Task<Models.HtmlText> GetHtmlTextAsync(int moduleId);

        Task<Models.HtmlText> GetHtmlTextAsync(int htmlTextId, int moduleId);

        Task AddHtmlTextAsync(Models.HtmlText htmltext);

        Task DeleteHtmlTextAsync(int htmlTextId, int moduleId);
    }
}
