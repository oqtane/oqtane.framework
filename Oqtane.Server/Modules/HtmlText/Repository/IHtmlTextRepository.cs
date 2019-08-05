using System.Collections.Generic;
using Oqtane.Shared.Modules.HtmlText.Models;

namespace Oqtane.Server.Modules.HtmlText.Repository
{
    public interface IHtmlTextRepository
    {
        IEnumerable<HtmlTextInfo> GetHtmlText();
        HtmlTextInfo AddHtmlText(HtmlTextInfo HtmlText);
        HtmlTextInfo UpdateHtmlText(HtmlTextInfo HtmlText);
        HtmlTextInfo GetHtmlText(int HtmlTextIdId);
        void DeleteHtmlText(int HtmlTextId);
    }
}
