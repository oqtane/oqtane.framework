using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextRepository
    {
        IEnumerable<Models.HtmlText> GetHtmlTexts(int moduleId);
        Models.HtmlText GetHtmlText(int htmlTextId);
        Models.HtmlText AddHtmlText(Models.HtmlText htmlText);
        void DeleteHtmlText(int htmlTextId);
    }
}
