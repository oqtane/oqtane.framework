using Oqtane.Documentation;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Repository
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextRepository
    {
        Models.HtmlText GetHtmlText(int moduleId);
        Models.HtmlText AddHtmlText(Models.HtmlText htmlText);
        Models.HtmlText UpdateHtmlText(Models.HtmlText htmlText);
        void DeleteHtmlText(int moduleId);
    }
}
