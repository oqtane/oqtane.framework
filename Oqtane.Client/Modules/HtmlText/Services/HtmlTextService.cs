using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.Modules.HtmlText.Models;

namespace Oqtane.Modules.HtmlText.Services
{
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public interface IHtmlTextService
    {
        Task<List<Models.HtmlText>> GetHtmlTextsAsync(int moduleId);

        Task<Models.HtmlText> GetHtmlTextAsync(int moduleId);

        Task<Models.HtmlText> GetHtmlTextAsync(int moduleId, int status);

        Task<Models.HtmlText> AddHtmlTextAsync(Models.HtmlText htmltext);
        Task<Models.HtmlText> UpdateHtmlTextAsync(Models.HtmlText htmltext);

        Task DeleteHtmlTextAsync(int htmlTextId, int moduleId);
    }

    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class HtmlTextService : ServiceBase, IHtmlTextService, IClientService
    {        
        public HtmlTextService(HttpClient http, SiteState siteState) : base(http, siteState) {}

        private string ApiUrl => CreateApiUrl("HtmlText");

        public async Task<List<Models.HtmlText>> GetHtmlTextsAsync(int moduleId)
        {
            return await GetJsonAsync<List<Models.HtmlText>>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<Models.HtmlText> GetHtmlTextAsync(int moduleId)
        {
            return await GetHtmlTextAsync(moduleId, WorkflowState.Published);
        }

        public async Task<Models.HtmlText> GetHtmlTextAsync(int moduleId, int status)
        {
            return await GetJsonAsync<Models.HtmlText>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{moduleId}?status={status}", EntityNames.Module, moduleId));
        }

        public async Task<Models.HtmlText> AddHtmlTextAsync(Models.HtmlText htmlText)
        {
            return await PostJsonAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}", EntityNames.Module, htmlText.ModuleId), htmlText);
        }

        public async Task<Models.HtmlText> UpdateHtmlTextAsync(Models.HtmlText htmlText)
        {
            return await PutJsonAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}", EntityNames.Module, htmlText.ModuleId), htmlText);
        }

        public async Task DeleteHtmlTextAsync(int htmlTextId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/{htmlTextId}/{moduleId}", EntityNames.Module, moduleId));
        }
    }
}
