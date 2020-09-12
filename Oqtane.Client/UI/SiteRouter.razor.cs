using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Oqtane.UI
{
    public partial class SiteRouter : IHandleAfterRender
    {
        private string _absoluteUri;
        private bool _navigationInterceptionEnabled;
        private PageState _pagestate;

        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;
        private readonly INavigationInterception _navigationInterception;
        private readonly IAliasService _aliasService;
        private readonly ITenantService _tenantService;
        private readonly ISiteService _siteService;
        private readonly IPageService _pageService;
        private readonly IUserService _userService;
        private readonly IModuleService _moduleService;
        private readonly ILogService _logService;

        public SiteRouter(
            AuthenticationStateProvider authenticationStateProvider,
            SiteState siteState,
            NavigationManager navigationManager,
            INavigationInterception navigationInterception,
            IAliasService aliasService,
            ITenantService tenantService,
            ISiteService siteService,
            IPageService pageService,
            IUserService userService,
            IModuleService moduleService,
            ILogService logService)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _siteState = siteState;
            _navigationManager = navigationManager;
            _navigationInterception = navigationInterception;
            _aliasService = aliasService;
            _tenantService = tenantService;
            _siteService = siteService;
            _pageService = pageService;
            _userService= userService;
            _moduleService = moduleService;
            _logService = logService;
        }

        [CascadingParameter]
        PageState PageState { get; set; }

        [Parameter]
        public Action<PageState> OnStateChange { get; set; }

        private RenderFragment DynamicComponent { get; set; }

        protected override void OnInitialized()
        {
            _absoluteUri = _navigationManager.Uri;
            _navigationManager.LocationChanged += LocationChanged;

            DynamicComponent = builder =>
            {
                if (PageState != null)
                {
                    builder.OpenComponent(0, Type.GetType(Constants.PageComponent));
                    builder.CloseComponent();
                }
            };
        }

        public void Dispose()
        {
            _navigationManager.LocationChanged -= LocationChanged;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (PageState == null)
            {
                // misconfigured api calls should not be processed through the router
                if (!_absoluteUri.Contains("~/api/"))
                {
                    await Refresh();
                }
                else
                {
                    Debug.WriteLine(GetType().FullName + ": Error: API call to " + _absoluteUri + " is not mapped to a Controller");
                }
            }
        }

        [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
        private async Task Refresh()
        {
            Alias alias = null;
            Site site;
            List<Page> pages;
            Page page;
            User user = null;
            List<Module> modules;
            var moduleid = -1;
            var action = string.Empty;
            var urlparameters = string.Empty;
            var editmode = false;
            var reload = Reload.None;
            var lastsyncdate = DateTime.UtcNow;
            var runtime = GetRuntime();

            Uri uri = new Uri(_absoluteUri);

            // get path
            var path = uri.LocalPath.Substring(1);

            // parse querystring
            var querystring = ParseQueryString(uri.Query);

            // the reload parameter is used to reload the PageState
            if (querystring.ContainsKey("reload"))
            {
                reload = Reload.Site;
            }

            if (PageState != null)
            {
                editmode = PageState.EditMode;
                lastsyncdate = PageState.LastSyncDate;
            }

            alias = await _aliasService.GetAliasAsync(path, lastsyncdate);
            _siteState.Alias = alias; // set state for services
            lastsyncdate = alias.SyncDate;

            // process any sync events for site or page
            if (reload != Reload.Site && alias.SyncEvents.Any())
            {
                if (PageState != null && alias.SyncEvents.Exists(item => item.EntityName == EntityNames.Page && item.EntityId == PageState.Page.PageId))
                {
                    reload = Reload.Page;
                }
                if (alias.SyncEvents.Exists(item => item.EntityName == EntityNames.Site && item.EntityId == alias.SiteId))
                {
                    reload = Reload.Site;
                }
            }

            if (reload == Reload.Site || PageState == null || alias.SiteId != PageState.Alias.SiteId)
            {
                site = await _siteService.GetSiteAsync(alias.SiteId);
                reload = Reload.Site;
            }
            else
            {
                site = PageState.Site;
            }

            if (site != null)
            {
                if (PageState == null || reload == Reload.Site)
                {
                    // get user
                    var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                    if (authState.User.Identity.IsAuthenticated)
                    {
                        user = await _userService.GetUserAsync(authState.User.Identity.Name, site.SiteId);
                    }
                }
                else
                {
                    user = PageState.User;
                }

                // process any sync events for user
                if (reload != Reload.Site && user != null && alias.SyncEvents.Any())
                {
                    if (alias.SyncEvents.Exists(item => item.EntityName == EntityNames.User && item.EntityId == user.UserId))
                    {
                        reload = Reload.Site;
                    }
                }

                if (PageState == null || reload >= Reload.Site)
                {
                    pages = await _pageService.GetPagesAsync(site.SiteId);
                }
                else
                {
                    pages = PageState.Pages;
                }

                // format path and remove alias
                path = path.Replace("//", "/");

                if (!path.EndsWith("/"))
                {
                    path += "/";
                }

                if (alias.Path != string.Empty)
                {
                    path = path.Substring(alias.Path.Length + 1);
                }

                // extract admin route elements from path
                var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                int result;

                int modIdPos = 0;
                int actionPos = 0;
                int urlParametersPos = 0;

                for (int i = 0; i < segments.Length; i++)
                {

                    if (segments[i] == Constants.UrlParametersDelimiter)
                    {
                        urlParametersPos = i + 1;
                    }

                    if (i >= urlParametersPos && urlParametersPos != 0)
                    {
                        urlparameters += "/" + segments[i];
                    }

                    if (segments[i] == Constants.ModuleDelimiter)
                    {
                        modIdPos = i + 1;
                        actionPos = modIdPos + 1;
                        if (actionPos > segments.Length - 1)
                        {
                            action = Constants.DefaultAction;
                        }
                        else
                        {
                            action = segments[actionPos];

                        }
                    }

                }

                // check if path has moduleid and action specification ie. pagename/moduleid/action/
                if (modIdPos > 0)
                {
                    int.TryParse(segments[modIdPos], out result);
                    moduleid = result;
                    if (actionPos > segments.Length - 1)
                    {
                        path = path.Replace(segments[modIdPos - 1] + "/" + segments[modIdPos] + "/", string.Empty);
                    }
                    else
                    {
                        path = path.Replace(segments[modIdPos - 1] + "/" + segments[modIdPos] + "/" + segments[actionPos] + "/", string.Empty);
                    }

                }

                if (urlParametersPos > 0)
                {
                    path = path.Replace(segments[urlParametersPos - 1] + urlparameters + "/", string.Empty);
                }

                // remove trailing slash so it can be used as a key for Pages
                if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

                if (PageState == null || reload >= Reload.Page)
                {
                    page = pages.Where(item => item.Path == path).FirstOrDefault();
                }
                else
                {
                    page = PageState.Page;
                }

                // failsafe in case router cannot locate the home page for the site
                if (page == null && path == string.Empty)
                {
                    page = pages.FirstOrDefault();
                    path = page.Path;
                }

                // check if page has changed
                if (page != null && page.Path != path)
                {
                    page = pages.Where(item => item.Path == path).FirstOrDefault();
                    reload = Reload.Page;
                    editmode = false;
                }

                if (page != null)
                {
                    if (PageState == null)
                    {
                        editmode = false;
                    }

                    // check if user is authorized to view page
                    if (UserSecurity.IsAuthorized(user, PermissionNames.View, page.Permissions))
                    {
                        page = await ProcessPage(page, site, user);

                        if (PageState != null && (PageState.ModuleId != moduleid || PageState.Action != action))
                        {
                            reload = Reload.Page;
                        }

                        if (PageState == null || reload >= Reload.Page)
                        {
                            modules = await _moduleService.GetModulesAsync(site.SiteId);
                            (page, modules) = ProcessModules(page, modules, moduleid, action, (!string.IsNullOrEmpty(page.DefaultContainerType)) ? page.DefaultContainerType : site.DefaultContainerType);
                        }
                        else
                        {
                            modules = PageState.Modules;
                        }

                        _pagestate = new PageState
                        {
                            Alias = alias,
                            Site = site,
                            Pages = pages,
                            Page = page,
                            User = user,
                            Modules = modules,
                            Uri = new Uri(_absoluteUri, UriKind.Absolute),
                            QueryString = querystring,
                            UrlParameters = urlparameters,
                            ModuleId = moduleid,
                            Action = action,
                            EditMode = editmode,
                            LastSyncDate = lastsyncdate,
                            Runtime = runtime
                        };

                        OnStateChange?.Invoke(_pagestate);
                    }
                }
                else
                {
                    if (user == null)
                    {
                        // redirect to login page
                        _navigationManager.NavigateTo(Utilities.NavigateUrl(alias.Path, "login", "returnurl=" + path));
                    }
                    else
                    {
                        await _logService.Log(null, null, user.UserId, GetType().AssemblyQualifiedName, Utilities.GetTypeNameLastSegment(GetType().AssemblyQualifiedName, 1), LogFunction.Security, LogLevel.Error, null, "Page Does Not Exist Or User Is Not Authorized To View Page {Path}", path);
                        if (path != string.Empty)
                        {
                            // redirect to home page
                            _navigationManager.NavigateTo(Utilities.NavigateUrl(alias.Path, string.Empty, string.Empty));
                        }
                    }
                }
            }
            else
            {
                // site does not exist
            }
        }

        private async void LocationChanged(object sender, LocationChangedEventArgs args)
        {
            _absoluteUri = args.Location;
            await Refresh();
        }

        Task IHandleAfterRender.OnAfterRenderAsync()
        {
            if (!_navigationInterceptionEnabled)
            {
                _navigationInterceptionEnabled = true;
                return _navigationInterception.EnableNavigationInterceptionAsync();
            }

            return Task.CompletedTask;
        }

        private Dictionary<string, string> ParseQueryString(string query)
        {
            Dictionary<string, string> querystring = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Substring(1); // ignore "?"
                foreach (string kvp in query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (kvp != string.Empty)
                    {
                        if (kvp.Contains("="))
                        {
                            string[] pair = kvp.Split('=');
                            querystring.Add(pair[0], pair[1]);
                        }
                        else
                        {
                            querystring.Add(kvp, "true"); // default parameter when no value is provided
                        }
                    }
                }
            }
            return querystring;
        }

        private async Task<Page> ProcessPage(Page page, Site site, User user)
        {
            try
            {
                if (page.IsPersonalizable && user != null)
                {
                    // load the personalized page
                    page = await _pageService.GetPageAsync(page.PageId, user.UserId);
                }

                if (string.IsNullOrEmpty(page.ThemeType))
                {
                    page.ThemeType = site.DefaultThemeType;
                    page.LayoutType = site.DefaultLayoutType;
                }

                page.Panes = new List<string>();
                page.Resources = new List<Resource>();

                string panes = string.Empty;
                Type themetype = Type.GetType(page.ThemeType);
                if (themetype == null)
                {
                    // fallback
                    page.ThemeType = Constants.DefaultTheme;
                    page.LayoutType = Constants.DefaultLayout;
                    themetype = Type.GetType(Constants.DefaultTheme);
                }
                if (themetype != null)
                {
                    var themeobject = Activator.CreateInstance(themetype) as IThemeControl;
                    if (themeobject != null)
                    {
                        panes = themeobject.Panes;
                        page.Resources = ManagePageResources(page.Resources, themeobject.Resources);
                    }
                }

                if (!string.IsNullOrEmpty(page.LayoutType))
                {
                    Type layouttype = Type.GetType(page.LayoutType);
                    if (layouttype != null)
                    {
                        var layoutobject = Activator.CreateInstance(layouttype) as IThemeControl;
                        if (layoutobject != null)
                        {
                            panes = layoutobject.Panes;
                        }
                    }
                }

                page.Panes = panes.Replace(";", ",").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch
            {
                // error loading theme or layout
            }

            return page;
        }

        private (Page Page, List<Module> Modules) ProcessModules(Page page, List<Module> modules, int moduleid, string action, string defaultcontainertype)
        {
            var paneindex = new Dictionary<string, int>();
            foreach (Module module in modules)
            {
                if (module.PageId == page.PageId || module.ModuleId == moduleid)
                {
                    var typename = string.Empty;
                    if (module.ModuleDefinition != null && (module.ModuleDefinition.Runtimes == string.Empty || module.ModuleDefinition.Runtimes.Contains(GetRuntime().ToString())))
                    {
                        typename = module.ModuleDefinition.ControlTypeTemplate;
                    }
                    else
                    {
                        typename = Constants.ErrorModule;
                    }

                    if (module.ModuleId == moduleid && action != string.Empty)
                    {
                        // check if the module defines custom routes
                        if (module.ModuleDefinition.ControlTypeRoutes != string.Empty)
                        {
                            foreach (string route in module.ModuleDefinition.ControlTypeRoutes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (route.StartsWith(action + "="))
                                {
                                    typename = route.Replace(action + "=", string.Empty);
                                }
                            }
                        }
                        module.ModuleType = typename.Replace(Constants.ActionToken, action);
                    }
                    else
                    {
                        module.ModuleType = typename.Replace(Constants.ActionToken, Constants.DefaultAction);
                    }

                    // get additional metadata from IModuleControl interface
                    typename = module.ModuleType;
                    if (Constants.DefaultModuleActions.Contains(action))
                    {
                        // core framework module action components
                        typename = Constants.DefaultModuleActionsTemplate.Replace(Constants.ActionToken, action);
                    }
                    Type moduletype = Type.GetType(typename);

                    // ensure component implements IModuleControl
                    if (moduletype != null && !moduletype.GetInterfaces().Contains(typeof(IModuleControl)))
                    {
                        module.ModuleType = string.Empty;
                    }
                    if (moduletype != null && module.ModuleType != string.Empty)
                    {
                        var moduleobject = Activator.CreateInstance(moduletype) as IModuleControl;
                        page.Resources = ManagePageResources(page.Resources, moduleobject.Resources);

                        // additional metadata needed for admin components
                        if (module.ModuleId == moduleid && action != string.Empty)
                        {
                            module.SecurityAccessLevel = moduleobject.SecurityAccessLevel;
                            module.ControlTitle = moduleobject.Title;
                            module.Actions = moduleobject.Actions;
                            module.UseAdminContainer = moduleobject.UseAdminContainer;
                        }
                    }

                    // ensure module's pane exists in current page and if not, assign it to the Admin pane
                    if (page.Panes == null || page.Panes.FindIndex(item => item.Equals(module.Pane, StringComparison.OrdinalIgnoreCase)) == -1)
                    {
                        module.Pane = Constants.AdminPane;
                    }

                    // calculate module position within pane
                    if (paneindex.ContainsKey(module.Pane.ToLower()))
                    {
                        paneindex[module.Pane.ToLower()] += 1;
                    }
                    else
                    {
                        paneindex.Add(module.Pane.ToLower(), 0);
                    }

                    module.PaneModuleIndex = paneindex[module.Pane.ToLower()];

                    if (string.IsNullOrEmpty(module.ContainerType))
                    {
                        module.ContainerType = defaultcontainertype;
                    }
                }
            }

            foreach (Module module in modules.Where(item => item.PageId == page.PageId))
            {
                module.PaneModuleCount = paneindex[module.Pane.ToLower()] + 1;
            }

            return (page, modules);
        }

        private List<Resource> ManagePageResources(List<Resource> pageresources, List<Resource> resources)
        {
            if (resources != null)
            {
                foreach (var resource in resources)
                {
                    // ensure resource does not exist already
                    if (pageresources.Find(item => item.Url == resource.Url) == null)
                    {
                        pageresources.Add(resource);
                    }
                }
            }
            return pageresources;
        }

        private Runtime GetRuntime()
    => RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"))
            ? Runtime.WebAssembly
            : Runtime.Server;
    }
}
