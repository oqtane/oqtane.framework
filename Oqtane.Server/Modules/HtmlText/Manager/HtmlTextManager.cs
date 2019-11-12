using Oqtane.Models;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using System.Net;

namespace Oqtane.Modules.HtmlText.Manager
{
    public class HtmlTextManager : IPortable
    {
        private IHtmlTextRepository htmltexts;

        public HtmlTextManager(IHtmlTextRepository htmltexts)
        {
            this.htmltexts = htmltexts;
        }

        public string ExportModule(Module Module)
        {
            string content = "";
            HtmlTextInfo htmltext = htmltexts.GetHtmlText(Module.ModuleId);
            if (htmltext != null)
            {
                content = WebUtility.HtmlEncode(htmltext.Content);
            }
            return content;
        }

        public void ImportModule(Module Module, string Content, string Version)
        {
            Content = WebUtility.HtmlDecode(Content);
            HtmlTextInfo htmltext = htmltexts.GetHtmlText(Module.ModuleId);
            if (htmltext != null)
            {
                htmltext.Content = Content;
                htmltexts.UpdateHtmlText(htmltext);
            }
            else
            {
                htmltext = new HtmlTextInfo();
                htmltext.ModuleId = Module.ModuleId;
                htmltext.Content = Content;
                htmltexts.AddHtmlText(htmltext);
            }
        }
    }
}
