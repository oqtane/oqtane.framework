using System.Collections.Generic;
using Oqtane.Shared.Modules.HtmlText.Models;

namespace Oqtane.Server.Modules.HtmlText.Repository
{
    public interface IHtmlTextRepository
    {
        HtmlTextInfo GetHtmlText(int ModuleId);
        HtmlTextInfo AddHtmlText(HtmlTextInfo HtmlText);
        HtmlTextInfo UpdateHtmlText(HtmlTextInfo HtmlText);
        void DeleteHtmlText(int HtmlTextId);
    }
}
