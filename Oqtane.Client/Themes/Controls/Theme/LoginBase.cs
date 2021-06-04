using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Providers;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.UI;

namespace Oqtane.Themes.Controls
{
    public class LoginBase : ThemeControlBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IUserService UserService { get; set; }
        [Inject] public IJSRuntime jsRuntime { get; set; }
        [Inject] public IServiceProvider ServiceProvider { get; set; }

        protected void LoginUser()
        {
            var returnurl = PageState.Alias.Path;
            if (PageState.Page.Path != "/")
            {
                returnurl += "/" + PageState.Page.Path;
            }
            NavigationManager.NavigateTo(NavigateUrl("login", "?returnurl=" + returnurl));
        }

        protected async Task LogoutUser()
        {
            await UserService.LogoutUserAsync(PageState.User);
            PageState.User = null;
            bool authorizedtoviewpage = UserSecurity.IsAuthorized(PageState.User, PermissionNames.View, PageState.Page.Permissions);

            if (PageState.Runtime == Oqtane.Shared.Runtime.Server)
            {
                // server-side Blazor
                var interop = new Interop(jsRuntime);
                string antiforgerytoken = await interop.GetElementByName("__RequestVerificationToken");
                var fields = new { __RequestVerificationToken = antiforgerytoken, returnurl = !authorizedtoviewpage ? PageState.Alias.Path : PageState.Alias.Path + "/" + PageState.Page.Path };
                string url = Utilities.TenantUrl(PageState.Alias, "/pages/logout/");
                await interop.SubmitForm(url, fields);
            }
            else
            {
                // client-side Blazor
                var authstateprovider = (IdentityAuthenticationStateProvider)ServiceProvider.GetService(typeof(IdentityAuthenticationStateProvider));
                authstateprovider.NotifyAuthenticationChanged();
                NavigationManager.NavigateTo(NavigateUrl(!authorizedtoviewpage ? PageState.Alias.Path : PageState.Page.Path, "reload"));
            }
        }
    }
}
