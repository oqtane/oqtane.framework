using System.Collections.Generic;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Repository
{
    public interface IHtmlTextRepository
    {
        HtmlTextInfo GetHtmlText(int ModuleId);
        HtmlTextInfo AddHtmlText(HtmlTextInfo HtmlText);
        HtmlTextInfo UpdateHtmlText(HtmlTextInfo HtmlText);
        void DeleteHtmlText(int ModuleId);
    }
}
