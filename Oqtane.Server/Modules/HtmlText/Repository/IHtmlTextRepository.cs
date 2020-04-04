using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Repository
{
    public interface IHtmlTextRepository
    {
        HtmlTextInfo GetHtmlText(int moduleId);
        HtmlTextInfo AddHtmlText(HtmlTextInfo htmlText);
        HtmlTextInfo UpdateHtmlText(HtmlTextInfo htmlText);
        void DeleteHtmlText(int moduleId);
    }
}
