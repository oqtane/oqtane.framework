using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Enums;
using Oqtane.Models;
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
        [Inject] public SiteState SiteState { get; set; }
        [Inject] public ILogService LoggingService { get; set; }

        protected void LoginUser()
        {
            Route route = new Route(PageState.Uri.AbsoluteUri, PageState.Alias.Path);
            NavigationManager.NavigateTo(NavigateUrl("login", "?returnurl=" + WebUtility.UrlEncode(route.PathAndQuery)));
        }

        protected async Task LogoutUser()
        {
            await LoggingService.Log(PageState.Alias, PageState.Page.PageId, null, PageState.User?.UserId, GetType().AssemblyQualifiedName, "Logout", LogFunction.Security, LogLevel.Information, null, "User Logout For Username {Username}", PageState.User?.Username);

            Route route = new Route(PageState.Uri.AbsoluteUri, PageState.Alias.Path);
            var url = route.PathAndQuery;

            // verify if anonymous users can access page
            if (!UserSecurity.IsAuthorized(null, PermissionNames.View, PageState.Page.PermissionList))
            {
                url = PageState.Alias.Path;
            }

            if (PageState.Runtime == Shared.Runtime.Hybrid)
            {
                // hybrid apps utilize an interactive logout
                await UserService.LogoutUserAsync(PageState.User);
                var authstateprovider = (IdentityAuthenticationStateProvider)ServiceProvider.GetService(typeof(IdentityAuthenticationStateProvider));
                authstateprovider.NotifyAuthenticationChanged();
                NavigationManager.NavigateTo(url, true);
            }
            else
            {
                // post to the Logout page to complete the logout process
                var fields = new { __RequestVerificationToken = SiteState.AntiForgeryToken, returnurl = url };
                var interop = new Interop(jsRuntime);
                await interop.SubmitForm(Utilities.TenantUrl(PageState.Alias, "/pages/logout/"), fields);
            }
        }
    }
}
