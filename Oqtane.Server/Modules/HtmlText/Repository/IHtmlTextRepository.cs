using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Documentation;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextRepository
    {
        IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId);
        Models.HtmlText GetHtmlText(int htmlTextId);
        Models.HtmlText AddHtmlText(Models.HtmlText htmlText);
        void DeleteHtmlText(int htmlTextId);

        Task<IEnumerable<Models.HtmlText>> GetHtmlTextsAsync(int moduleId);
        Task<Models.HtmlText> GetHtmlTextAsync(int htmlTextId);
        Task<Models.HtmlText> AddHtmlTextAsync(Models.HtmlText htmlText);
        Task DeleteHtmlTextAsync(int htmlTextId);
    }
}
