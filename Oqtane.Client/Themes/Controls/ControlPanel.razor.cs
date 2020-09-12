using Microsoft.AspNetCore.Components;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oqtane.Themes.Controls
{
    public partial class ControlPanel : ThemeControlBase
    {
        private readonly NavigationManager _navigationManager;
        private readonly IUserService _userService;
        private readonly IModuleDefinitionService _moduleDefinitionService;
        private readonly IThemeService _themeService;
        private readonly IModuleService _moduleService;
        private readonly IPageService _pageService;
        private readonly IPageModuleService _pageModuleService;
        private readonly ILogService _logger;
        private readonly ISettingService _settingService;

        public ControlPanel(
            NavigationManager navigationManager,
            IUserService userService,
            IModuleDefinitionService moduleDefinitionService,
            IThemeService themeService,
            IModuleService moduleService,
            IPageService pageService,
            IPageModuleService pageModuleService,
            ILogService logger,
            ISettingService settingService)
        {
            _navigationManager = navigationManager;
            _userService = userService;
            _moduleDefinitionService = moduleDefinitionService;
            _themeService = themeService;
            _moduleService = moduleService;
            _pageService = pageService;
            _pageModuleService = pageModuleService;
            _logger = logger;
            _settingService = settingService;
        }

        private bool _deleteConfirmation = false;
        private List<string> _categories = new List<string>();
        private List<ModuleDefinition> _allModuleDefinitions;
        private List<ModuleDefinition> _moduleDefinitions;
        private List<Page> _pages = new List<Page>();
        private List<Module> _modules = new List<Module>();
        private List<ThemeControl> _containers = new List<ThemeControl>();
        private string _display = "display: none;";
        private string _category = "Common";

        protected string PageId { get; private set; } = "-";

        protected string ModuleId { get; private set; } = "-";

        protected string ModuleType { get; private set; } = "new";

        protected string ModuleDefinitionName { get; private set; } = "-";

        protected string Category
        {
            get => _category;
            private set
            {
                if (_category != value)
                {
                    _category = value;
                    _moduleDefinitions = _allModuleDefinitions.Where(item => item.Categories.Contains(Category)).ToList();
                    ModuleDefinitionName = "-";
                    Description = "";
                    StateHasChanged();
                    _ = UpdateSettingsAsync();
                }
            }
        }

        protected string Pane
        {
            get => _pane;
            private set
            {
                if (_pane != value)
                {
                    _pane = value;
                    _ = UpdateSettingsAsync();
                }
            }
        }

        protected string Description { get; private set; }

        protected string Title { get; private set; }

        protected string ContainerType { get; private set; }

        protected string Message { get; private set; }

        [Parameter]
        public string ButtonClass { get; set; } = "btn-outline-secondary";

        [Parameter]
        public string CardClass { get; set; } = "card border-secondary mb-3";

        [Parameter]
        public string HeaderClass { get; set; } = "card-header";

        [Parameter]
        public string BodyClass { get; set; } = "card-body";

        protected override async Task OnInitializedAsync()
        {
            if (UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, PageState.Page.Permissions))
            {
                _pages?.Clear();

                foreach (Page p in PageState.Pages)
                {
                    if (UserSecurity.IsAuthorized(PageState.User, PermissionNames.View, p.Permissions))
                    {
                        _pages.Add(p);
                    }
                }
                await LoadSettingsAsync();

                var themes = await _themeService.GetThemesAsync();
                _containers = _themeService.GetContainerControls(themes, PageState.Page.ThemeType);
                ContainerType = PageState.Site.DefaultContainerType;
                _allModuleDefinitions = await _moduleDefinitionService.GetModuleDefinitionsAsync(PageState.Site.SiteId);
                _moduleDefinitions = _allModuleDefinitions.Where(item => item.Categories.Contains(Category)).ToList();
                _categories = _allModuleDefinitions.SelectMany(m => m.Categories.Split(',')).Distinct().ToList();
            }
        }

        private void CategoryChanged(ChangeEventArgs e)
        {
            Category = (string)e.Value;
        }

        private void ModuleChanged(ChangeEventArgs e)
        {
            ModuleDefinitionName = (string)e.Value;
            if (ModuleDefinitionName != "-")
            {
                var moduleDefinition = _moduleDefinitions.FirstOrDefault(item => item.ModuleDefinitionName == ModuleDefinitionName);
                Description = "<br /><div class=\"alert alert-info\" role=\"alert\">" + moduleDefinition.Description + "</div>";
            }
            else
            {
                Description = "";
            }

            StateHasChanged();
        }

        private void PageChanged(ChangeEventArgs e)
        {
            PageId = (string)e.Value;
            if (PageId != "-")
            {
                _modules = PageState.Modules
                    .Where(module => module.PageId == int.Parse(PageId)
                                     && !module.IsDeleted
                                     && UserSecurity.IsAuthorized(PageState.User, PermissionNames.View, module.Permissions))
                    .ToList();
            }
            ModuleId = "-";
            StateHasChanged();
        }

        private async Task AddModule()
        {
            if (UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, PageState.Page.Permissions))
            {
                if ((ModuleType == "new" && ModuleDefinitionName != "-") || (ModuleType != "new" && ModuleId != "-"))
                {
                    if (ModuleType == "new")
                    {
                        Module module = new Module();
                        module.SiteId = PageState.Site.SiteId;
                        module.PageId = PageState.Page.PageId;
                        module.ModuleDefinitionName = ModuleDefinitionName;
                        module.AllPages = false;
                        module.Permissions = PageState.Page.Permissions;
                        module = await _moduleService.AddModuleAsync(module);
                        ModuleId = module.ModuleId.ToString();
                    }

                    var pageModule = new PageModule
                    {
                        PageId = PageState.Page.PageId,
                        ModuleId = int.Parse(ModuleId),
                        Title = Title
                    };
                    if (pageModule.Title == "")
                    {
                        if (ModuleType == "new")
                        {
                            pageModule.Title = _moduleDefinitions.FirstOrDefault(item => item.ModuleDefinitionName == ModuleDefinitionName)?.Name;
                        }
                        else
                        {
                            pageModule.Title = _modules.FirstOrDefault(item => item.ModuleId == int.Parse(ModuleId))?.Title;
                        }
                    }

                    pageModule.Pane = Pane;
                    pageModule.Order = int.MaxValue;
                    pageModule.ContainerType = ContainerType;

                    if (pageModule.ContainerType == PageState.Site.DefaultContainerType)
                    {
                        pageModule.ContainerType = "";
                    }

                    await _pageModuleService.AddPageModuleAsync(pageModule);
                    await _pageModuleService.UpdatePageModuleOrderAsync(pageModule.PageId, pageModule.Pane);

                    Message = "<br /><div class=\"alert alert-success\" role=\"alert\">Module Added To Page</div>";
                    _navigationManager.NavigateTo(NavigateUrl());
                }
                else
                {
                    Message = "<br /><div class=\"alert alert-warning\" role=\"alert\">You Must Select A Module</div>";
                }
            }
            else
            {
                Message = "<br /><div class=\"alert alert-error\" role=\"alert\">Not Authorized</div>";
            }
        }

        private async Task ToggleEditMode(bool EditMode)
        {
            if (UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, PageState.Page.Permissions))
            {
                if (EditMode)
                {
                    PageState.EditMode = false;
                }
                else
                {
                    PageState.EditMode = true;
                }

                _navigationManager.NavigateTo(NavigateUrl(PageState.Page.Path, "edit=" + ((PageState.EditMode) ? "1" : "0")));
            }
            else
            {
                if (PageState.Page.IsPersonalizable && PageState.User != null)
                {
                    await _pageService.AddPageAsync(PageState.Page.PageId, PageState.User.UserId);
                    PageState.EditMode = true;
                    _navigationManager.NavigateTo(NavigateUrl(PageState.Page.Path, "edit=" + ((PageState.EditMode) ? "1" : "0")));
                }
            }
        }

        private void ShowControlPanel()
        {
            Message = "";
            _display = "width: 25%; min-width: 375px;";
            StateHasChanged();
        }

        private void HideControlPanel()
        {
            Message = "";
            _display = "width: 0%;";
            StateHasChanged();
        }

        private void Navigate(string location)
        {
            HideControlPanel();
            Module module;
            switch (location)
            {
                case "Admin":
                    // get admin dashboard moduleid
                    module = PageState.Modules.FirstOrDefault(item => item.ModuleDefinitionName == Constants.AdminDashboardModule);

                    if (module != null)
                    {
                        _navigationManager.NavigateTo(EditUrl(PageState.Page.Path, module.ModuleId, "Index", ""));
                    }

                    break;
                case "Add":
                case "Edit":
                    string url = "";
                    // get page management moduleid
                    module = PageState.Modules.FirstOrDefault(item => item.ModuleDefinitionName == Constants.PageManagementModule);

                    if (module != null)
                    {
                        switch (location)
                        {
                            case "Add":
                                url = EditUrl(PageState.Page.Path, module.ModuleId, location, "");
                                break;
                            case "Edit":
                                url = EditUrl(PageState.Page.Path, module.ModuleId, location, "id=" + PageState.Page.PageId.ToString());
                                break;
                        }
                    }

                    if (url != "")
                    {
                        _navigationManager.NavigateTo(url);
                    }

                    break;
            }
        }

        private async void Publish(string action)
        {
            if (UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, PageState.Page.Permissions))
            {
                List<PermissionString> permissions;

                if (action == "publish")
                {
                    // publish all modules
                    foreach (var module in PageState.Modules.Where(item => item.PageId == PageState.Page.PageId))
                    {
                        permissions = UserSecurity.GetPermissionStrings(module.Permissions);
                        foreach (var permissionstring in permissions)
                        {
                            if (permissionstring.PermissionName == PermissionNames.View)
                            {
                                List<string> ids = permissionstring.Permissions.Split(';').ToList();
                                if (!ids.Contains(Constants.AllUsersRole)) ids.Add(Constants.AllUsersRole);
                                if (!ids.Contains(Constants.RegisteredRole)) ids.Add(Constants.RegisteredRole);
                                permissionstring.Permissions = string.Join(";", ids.ToArray());
                            }
                        }
                        module.Permissions = UserSecurity.SetPermissionStrings(permissions);
                        await _moduleService.UpdateModuleAsync(module);
                    }
                }

                // publish page
                var page = PageState.Page;
                permissions = UserSecurity.GetPermissionStrings(page.Permissions);
                foreach (var permissionstring in permissions)
                {
                    if (permissionstring.PermissionName == PermissionNames.View)
                    {
                        List<string> ids = permissionstring.Permissions.Split(';').ToList();
                        switch (action)
                        {
                            case "publish":
                                if (!ids.Contains(Constants.AllUsersRole)) ids.Add(Constants.AllUsersRole);
                                if (!ids.Contains(Constants.RegisteredRole)) ids.Add(Constants.RegisteredRole);
                                break;
                            case "unpublish":
                                ids.Remove(Constants.AllUsersRole);
                                ids.Remove(Constants.RegisteredRole);
                                break;
                        }
                        permissionstring.Permissions = string.Join(";", ids.ToArray());
                    }
                }
                page.Permissions = UserSecurity.SetPermissionStrings(permissions);
                await _pageService.UpdatePageAsync(page);
                _navigationManager.NavigateTo(NavigateUrl(PageState.Page.Path, "reload"));
            }
        }

        private void ConfirmDelete()
        {
            _deleteConfirmation = !_deleteConfirmation;
            StateHasChanged();
        }

        private async Task DeletePage()
        {
            ConfirmDelete();

            var page = PageState.Page;
            try
            {
                if (page.UserId == null)
                {
                    page.IsDeleted = true;
                    await _pageService.UpdatePageAsync(page);
                    await _logger.Log(page.PageId, null, PageState.User.UserId, GetType().AssemblyQualifiedName, "ControlPanel", LogFunction.Delete, LogLevel.Information, null, "Page Deleted {Page}", page);
                    _navigationManager.NavigateTo(NavigateUrl(""));
                }
                else // personalized page
                {
                    await _pageService.DeletePageAsync(page.PageId);
                    await _logger.Log(page.PageId, null, PageState.User.UserId, GetType().AssemblyQualifiedName, "ControlPanel", LogFunction.Delete, LogLevel.Information, null, "Page Deleted {Page}", page);
                    _navigationManager.NavigateTo(NavigateUrl());
                }
            }
            catch (Exception ex)
            {
                await _logger.Log(page.PageId, null, PageState.User.UserId, GetType().AssemblyQualifiedName, "ControlPanel", LogFunction.Delete, LogLevel.Information, ex, "Page Deleted {Page} {Error}", page, ex.Message);
            }
        }

        private string settingCategory = "CP-category";
        private string settingPane = "CP-pane";
        private string _pane = "";

        private async Task LoadSettingsAsync()
        {
            Dictionary<string, string> settings = await _settingService.GetUserSettingsAsync(PageState.User.UserId);
            _category = _settingService.GetSetting(settings, settingCategory, "Common");
            var pane = _settingService.GetSetting(settings, settingPane, "");
            _pane = PageState.Page.Panes.Contains(pane) ? pane : PageState.Page.Panes.FirstOrDefault();
        }

        private async Task UpdateSettingsAsync()
        {
            Dictionary<string, string> settings = await _settingService.GetUserSettingsAsync(PageState.User.UserId);
            _settingService.SetSetting(settings, settingCategory, _category);
            _settingService.SetSetting(settings, settingPane, _pane);
            await _settingService.UpdateUserSettingsAsync(settings, PageState.User.UserId);
        }
    }
}
