using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Modules.HtmlText.Services
{
    public class HtmlTextService : ServiceBase, IHtmlTextService
    {
        
        private readonly NavigationManager _navigationManager;
        private readonly SiteState _siteState;

        public HtmlTextService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string ApiUrl => CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "HtmlText");

        public async Task<HtmlTextInfo> GetHtmlTextAsync(int moduleId)
        {
            HtmlTextInfo htmlText;
            try
            {
                //because GetJsonAsync() returns an error if no content exists for the ModuleId ( https://github.com/aspnet/AspNetCore/issues/14041 )
                //null value is transfered as empty list
                var htmlTextList = await GetJsonAsync<List<HtmlTextInfo>>(ApiUrl + "/" + moduleId + "?entityid=" + moduleId);
                htmlText = htmlTextList.FirstOrDefault();
            }
            catch
            {
                htmlText = null;
            }

            return htmlText;
        }

        public async Task AddHtmlTextAsync(HtmlTextInfo htmlText)
        {
            await PostJsonAsync(ApiUrl + "?entityid=" + htmlText.ModuleId, htmlText);
        }

        public async Task UpdateHtmlTextAsync(HtmlTextInfo htmlText)
        {
            await PutJsonAsync(ApiUrl + "/" + htmlText.HtmlTextId + "?entityid=" + htmlText.ModuleId, htmlText);
        }


        public async Task DeleteHtmlTextAsync(int moduleId)
        {
            await DeleteAsync(ApiUrl + "/" + moduleId + "?entityid=" + moduleId);
        }
    }
}
