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
        [Inject] public ISettingService SettingService { get; set; }
        [Inject] public IJSRuntime jsRuntime { get; set; }
        [Inject] public IServiceProvider ServiceProvider { get; set; }

        protected void LoginUser()
        {
            var allowexternallogin = (SettingService.GetSetting(PageState.Site.Settings, "ExternalLogin:ProviderType", "") != "") ? true : false;
            var allowsitelogin = bool.Parse(SettingService.GetSetting(PageState.Site.Settings, "LoginOptions:AllowSiteLogin", "true"));

            var returnurl = WebUtility.UrlEncode(PageState.Route.PathAndQuery);

            if (allowexternallogin && !allowsitelogin)
            {
                // external login
                NavigationManager.NavigateTo(Utilities.TenantUrl(PageState.Alias, "/pages/external?returnurl=" + returnurl), true);
            }
            else
            {
                // local login
                NavigationManager.NavigateTo(NavigateUrl("login", "?returnurl=" + returnurl));
            }
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
