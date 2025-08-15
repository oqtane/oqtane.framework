using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Application.Services
{
    public interface IMyModuleService
    {
        Task<List<Models.MyModule>> GetMyModulesAsync(int ModuleId);

        Task<Models.MyModule> GetMyModuleAsync(int MyModuleId, int ModuleId);

        Task<Models.MyModule> AddMyModuleAsync(Models.MyModule MyModule);

        Task<Models.MyModule> UpdateMyModuleAsync(Models.MyModule MyModule);

        Task DeleteMyModuleAsync(int MyModuleId, int ModuleId);
    }

    public class MyModuleService : ServiceBase, IMyModuleService
    {
        public MyModuleService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("MyModule");

        public async Task<List<Models.MyModule>> GetMyModulesAsync(int ModuleId)
        {
            List<Models.MyModule> Tasks = await GetJsonAsync<List<Models.MyModule>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.MyModule>().ToList());
            return Tasks.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.MyModule> GetMyModuleAsync(int MyModuleId, int ModuleId)
        {
            return await GetJsonAsync<Models.MyModule>(CreateAuthorizationPolicyUrl($"{Apiurl}/{MyModuleId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.MyModule> AddMyModuleAsync(Models.MyModule MyModule)
        {
            return await PostJsonAsync<Models.MyModule>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, MyModule.ModuleId), MyModule);
        }

        public async Task<Models.MyModule> UpdateMyModuleAsync(Models.MyModule MyModule)
        {
            return await PutJsonAsync<Models.MyModule>(CreateAuthorizationPolicyUrl($"{Apiurl}/{MyModule.MyModuleId}", EntityNames.Module, MyModule.ModuleId), MyModule);
        }

        public async Task DeleteMyModuleAsync(int MyModuleId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{MyModuleId}/{ModuleId}", EntityNames.Module, ModuleId));
        }
    }
}
