using Oqtane.Models;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using System.Net;

namespace Oqtane.Modules.HtmlText.Manager
{
    public class HtmlTextManager : IPortable
    {
        private IHtmlTextRepository _htmlTexts;

        public HtmlTextManager(IHtmlTextRepository htmltexts)
        {
            _htmlTexts = htmltexts;
        }

        public string ExportModule(Module module)
        {
            string content = "";
            HtmlTextInfo htmltext = _htmlTexts.GetHtmlText(module.ModuleId);
            if (htmltext != null)
            {
                content = WebUtility.HtmlEncode(htmltext.Content);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
        {
            content = WebUtility.HtmlDecode(content);
            HtmlTextInfo htmltext = _htmlTexts.GetHtmlText(module.ModuleId);
            if (htmltext != null)
            {
                htmltext.Content = content;
                _htmlTexts.UpdateHtmlText(htmltext);
            }
            else
            {
                htmltext = new HtmlTextInfo();
                htmltext.ModuleId = module.ModuleId;
                htmltext.Content = content;
                _htmlTexts.AddHtmlText(htmltext);
            }
        }
    }
}
