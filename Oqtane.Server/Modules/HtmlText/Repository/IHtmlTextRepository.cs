using System.Collections.Generic;
using Oqtane.Shared.Modules.HtmlText.Models;

namespace Oqtane.Server.Modules.HtmlText.Repository
{
    public interface IHtmlTextRepository
    {
        IEnumerable<HtmlTextInfo> GetHtmlText();
        void AddHtmlText(HtmlTextInfo HtmlText);
        void UpdateHtmlText(HtmlTextInfo HtmlText);
        HtmlTextInfo GetHtmlText(int HtmlTextIdId);
        void DeleteHtmlText(int HtmlTextId);
    }
}
