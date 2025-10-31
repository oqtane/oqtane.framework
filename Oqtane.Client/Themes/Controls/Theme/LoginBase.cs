using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Enums;
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

        private bool allowexternallogin;
        private bool allowsitelogin;
        protected string loginurl;
        protected string logouturl;
        protected string returnurl;
        protected string everywhere;

        protected override void OnParametersSet()
        {
            allowexternallogin = (SettingService.GetSetting(PageState.Site.Settings, "ExternalLogin:ProviderType", "") != "") ? true : false;
            allowsitelogin = bool.Parse(SettingService.GetSetting(PageState.Site.Settings, "LoginOptions:AllowSiteLogin", "true"));

            // set login url
            if (allowexternallogin && !allowsitelogin)
            {
                // external login
                loginurl = Utilities.TenantUrl(PageState.Alias, "/pages/external");
            }
            else
            {
                // local login
                loginurl = NavigateUrl("login");
            }

            if (!PageState.QueryString.ContainsKey("returnurl"))
            {
                // remember current url
                loginurl += "?returnurl=" + WebUtility.UrlEncode(PageState.Route.PathAndQuery);
            }
            else
            {
                // use existing value
                loginurl += "?returnurl=" + PageState.QueryString["returnurl"];
            }

            // set logout url
            logouturl = Utilities.TenantUrl(PageState.Alias, "/pages/logout/");
            everywhere = SettingService.GetSetting(PageState.Site.Settings, "LoginOptions:LogoutEverywhere", "false");

            // verify anonymous users can access current page
            if (UserSecurity.IsAuthorized(null, PermissionNames.View, PageState.Page.PermissionList) && Utilities.IsEffectiveAndNotExpired(PageState.Page.EffectiveDate, PageState.Page.ExpiryDate))
            {
                if (PageState.Route.Action != Constants.DefaultAction && PageState.Modules.Any() && PageState.Modules.First().SecurityAccessLevel > SecurityAccessLevel.View)
                {
                    returnurl = PageState.Route.PagePath;
                }
                else
                {
                    returnurl = PageState.Route.PathAndQuery;
                }
            }
            else
            {
                returnurl = PageState.Alias.Path;
            }
        }

        protected void LoginUser()
        {
            if (allowexternallogin && !allowsitelogin)
            {
                // external login
                NavigationManager.NavigateTo(loginurl, true);
            }
            else
            {
                // local login
                NavigationManager.NavigateTo(loginurl);
            }
        }

        protected async Task LogoutUser()
        {
            await LoggingService.Log(PageState.Alias, PageState.Page.PageId, null, PageState.User?.UserId, GetType().AssemblyQualifiedName, "Logout", LogFunction.Security, LogLevel.Information, null, "User Logout For Username {Username}", PageState.User?.Username);

            if (PageState.Runtime == Runtime.Hybrid)
            {
                // hybrid apps utilize an interactive logout
                await UserService.LogoutUserAsync(PageState.User);
                var authstateprovider = (IdentityAuthenticationStateProvider)ServiceProvider.GetService(typeof(IdentityAuthenticationStateProvider));
                authstateprovider.NotifyAuthenticationChanged();
                NavigationManager.NavigateTo(returnurl, true);
            }
            else // this condition is only valid for legacy Login button inheriting from LoginBase
            {
                // post to the Logout page to complete the logout process
                var fields = new { __RequestVerificationToken = SiteState.AntiForgeryToken, returnurl = returnurl, everywhere = bool.Parse(SettingService.GetSetting(PageState.Site.Settings, "LoginOptions:LogoutEverywhere", "false")) };
                var interop = new Interop(jsRuntime);
                await interop.SubmitForm(logouturl, fields);
            }
        }
    }
}
