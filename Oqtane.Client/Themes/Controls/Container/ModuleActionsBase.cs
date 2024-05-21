using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.UI;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Localization;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable MemberCanBePrivate.Global

namespace Oqtane.Themes.Controls
{
    public class ModuleActionsBase : ComponentBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IPageModuleService PageModuleService { get; set; }
        [Inject] public IModuleService ModuleService { get; set; }
        [Inject] public IStringLocalizer<ModuleActionsBase> Localizer { get; set; }

        [Parameter] public PageState PageState { get; set; }
        [Parameter] public Module ModuleState { get; set; }

        public List<ActionViewModel> Actions;

        protected override void OnParametersSet()
        {
            Actions = GetActions();
        }

        protected virtual List<ActionViewModel> GetActions()
        {
            var actionList = new List<ActionViewModel>();

            if (PageState.EditMode && UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, PageState.Page.PermissionList))
            {
                actionList.Add(new ActionViewModel { Icon = Icons.Cog, Name = Localizer["ManageSettings"], Action = async (u, m) => await Settings(u, m) });

                if (UserSecurity.ContainsRole(ModuleState.PermissionList, PermissionNames.View, RoleNames.Everyone))
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.CircleX, Name = Localizer["UnpublishModule"], Action = async (s, m) => await Unpublish(s, m) });
                }
                else
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.CircleCheck, Name = Localizer["PublishModule"], Action = async (s, m) => await Publish(s, m) });
                }
                actionList.Add(new ActionViewModel { Icon = Icons.Trash, Name = Localizer["DeleteModule"], Action = async (u, m) => await DeleteModule(u, m) });

                if (ModuleState.ModuleDefinition != null && ModuleState.ModuleDefinition.IsPortable)
                {
                    actionList.Add(new ActionViewModel { Name = "" });
                    actionList.Add(new ActionViewModel { Icon = Icons.CloudUpload, Name = Localizer["ImportContent"], Action = async (u, m) => await EditUrlAsync(u, m.ModuleId, "Import") });
                    actionList.Add(new ActionViewModel { Icon = Icons.CloudDownload, Name = Localizer["ExportContent"], Action = async (u, m) => await EditUrlAsync(u, m.ModuleId, "Export") });
                }

                actionList.Add(new ActionViewModel { Name = "" });

                if (ModuleState.PaneModuleIndex > 0)
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.DataTransferUpload, Name = Localizer["MoveToTop"], Action = async (s, m) => await MoveTop(s, m) });
                }

                if (ModuleState.PaneModuleIndex > 0)
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.ArrowThickTop, Name = "Move Up", Action = async (s, m) => await MoveUp(s, m) });
                }

                if (ModuleState.PaneModuleIndex < (ModuleState.PaneModuleCount - 1))
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.ArrowThickBottom, Name = "Move Down", Action = async (s, m) => await MoveDown(s, m) });
                }

                if (ModuleState.PaneModuleIndex < (ModuleState.PaneModuleCount - 1))
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.DataTransferDownload, Name = "Move To Bottom", Action = async (s, m) => await MoveBottom(s, m) });
                }

                foreach (string pane in PageState.Page.Panes)
                {
                    if (pane != ModuleState.Pane)
                    {
                        actionList.Add(new ActionViewModel { Icon = Icons.AccountLogin, Name = pane + " Pane", Action = async (s, m) => await MoveToPane(s, pane, m) });
                    }
                }
            }

            return actionList;
        }

        private async Task<string> EditUrlAsync(string url, int moduleId, string import)
        {
            await Task.Yield();
            return Utilities.EditUrl(PageState.Alias.Path, PageState.Page.Path, moduleId, import, "");
        }

        protected async Task ModuleAction(ActionViewModel action)
        {
            if (PageState.EditMode && UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, ModuleState.PermissionList))
            {
                PageModule pagemodule = await PageModuleService.GetPageModuleAsync(ModuleState.PageModuleId);

                string url = Utilities.NavigateUrl(PageState.Alias.Path, PageState.Page.Path, "edit=true&refresh");

                if (action.Action != null)
                {
                    url = await action.Action(url, pagemodule);
                }

                NavigationManager.NavigateTo(url);
            }
        }

        private async Task<string> MoveToPane(string url, string newPane, PageModule pagemodule)
        {
            string oldPane = pagemodule.Pane;
            pagemodule.Pane = newPane;
            pagemodule.Order = int.MaxValue; // add to bottom of pane
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, oldPane);
            return url;
        }

        private async Task<string> DeleteModule(string url, PageModule pagemodule)
        {
            pagemodule.IsDeleted = true;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return url;
        }

        private async Task<string> Settings(string url, PageModule pagemodule)
        {
            await Task.Yield();
            var returnurl = Utilities.NavigateUrl(PageState.Alias.Path, PageState.Page.Path, "edit=true");
            url = Utilities.EditUrl(PageState.Alias.Path, PageState.Page.Path, pagemodule.ModuleId, "Settings", "returnurl=" + WebUtility.UrlEncode(returnurl));
            return url;
        }

        private async Task<string> Publish(string url, PageModule pagemodule)
        {
            var permissions = pagemodule.Module.PermissionList;
            if (!permissions.Any(item => item.PermissionName == PermissionNames.View && item.RoleName == RoleNames.Everyone))
            {
                permissions.Add(new Permission(ModuleState.SiteId, EntityNames.Module, pagemodule.ModuleId, PermissionNames.View, RoleNames.Everyone, null, true));
            }
            if (!permissions.Any(item => item.PermissionName == PermissionNames.View && item.RoleName == RoleNames.Registered))
            {
                permissions.Add(new Permission(ModuleState.SiteId, EntityNames.Module, pagemodule.ModuleId, PermissionNames.View, RoleNames.Registered, null, true));
            }
            pagemodule.Module.PermissionList = permissions;
            await ModuleService.UpdateModuleAsync(pagemodule.Module);
            return url;
        }

        private async Task<string> Unpublish(string url, PageModule pagemodule)
        {
            var permissions = pagemodule.Module.PermissionList;
            if (permissions.Any(item => item.PermissionName == PermissionNames.View && item.RoleName == RoleNames.Everyone))
            {
                permissions.Remove(permissions.First(item => item.PermissionName == PermissionNames.View && item.RoleName == RoleNames.Everyone));
            }
            if (permissions.Any(item => item.PermissionName == PermissionNames.View && item.RoleName == RoleNames.Registered))
            {
                permissions.Remove(permissions.First(item => item.PermissionName == PermissionNames.View && item.RoleName == RoleNames.Registered));
            }
            pagemodule.Module.PermissionList = permissions;
            await ModuleService.UpdateModuleAsync(pagemodule.Module);
            return url;
        }

        private async Task<string> MoveTop(string url, PageModule pagemodule)
        {
            pagemodule.Order = 0;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return url;
        }

        private async Task<string> MoveBottom(string url, PageModule pagemodule)
        {
            pagemodule.Order = int.MaxValue;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return url;
        }

        private async Task<string> MoveUp(string url, PageModule pagemodule)
        {
            pagemodule.Order -= 3;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return url;
        }

        private async Task<string> MoveDown(string url, PageModule pagemodule)
        {
            pagemodule.Order += 3;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return url;
        }

        public class ActionViewModel
        {
            public string Icon { get; set; }
            public string Name { set; get; }
            public Func<string, PageModule, Task<string>> Action { set; get; }
        }
    }
}
