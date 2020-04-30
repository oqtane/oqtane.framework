using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Modules.HtmlText.Repository;
using System.Net;

namespace Oqtane.Modules.HtmlText.Manager
{
    public class HtmlTextManager : IInstallable, IPortable
    {
        private IHtmlTextRepository _htmlTexts;
        private ISqlRepository _sql;

        public HtmlTextManager(IHtmlTextRepository htmltexts, ISqlRepository sql)
        {
            _htmlTexts = htmltexts;
            _sql = sql;
        }

        public bool Install(Tenant tenant, string version)
        {
            return _sql.ExecuteScript(tenant, GetType().Assembly, "HtmlText." + version + ".sql");
        }

        public bool Uninstall(Tenant tenant)
        {
            return _sql.ExecuteScript(tenant, GetType().Assembly, "HtmlText.Uninstall.sql");
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
