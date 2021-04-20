using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Repository
{
    public interface IHtmlTextRepository
    {
        Models.HtmlText GetHtmlText(int moduleId);
        Models.HtmlText AddHtmlText(Models.HtmlText htmlText);
        Models.HtmlText UpdateHtmlText(Models.HtmlText htmlText);
        void DeleteHtmlText(int moduleId);
    }
}
