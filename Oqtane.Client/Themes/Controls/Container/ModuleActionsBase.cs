using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable MemberCanBePrivate.Global

namespace Oqtane.Themes.Controls
{
    public class ModuleActionsBase : ContainerBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IPageModuleService PageModuleService { get; set; }
        [Inject] public IModuleService ModuleService { get; set; }

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
                actionList.Add(new ActionViewModel { Icon = Icons.Cog, Name = "Manage Settings", Action = async (u, m) => await Settings(u, m) });

                if (UserSecurity.ContainsRole(ModuleState.PermissionList, PermissionNames.View, RoleNames.Everyone))
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.CircleX, Name = "Unpublish Module", Action = async (s, m) => await Unpublish(s, m) });
                }
                else
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.CircleCheck, Name = "Publish Module", Action = async (s, m) => await Publish(s, m) });
                }
                actionList.Add(new ActionViewModel { Icon = Icons.Trash, Name = "Delete Module", Action = async (u, m) => await DeleteModule(u, m) });

                if (ModuleState.ModuleDefinition != null && ModuleState.ModuleDefinition.IsPortable)
                {
                    actionList.Add(new ActionViewModel { Name = "" });
                    actionList.Add(new ActionViewModel { Icon = Icons.CloudUpload, Name = "Import Content", Action = async (u, m) => await EditUrlAsync(u, m.ModuleId, "Import") });
                    actionList.Add(new ActionViewModel { Icon = Icons.CloudDownload, Name = "Export Content", Action = async (u, m) => await EditUrlAsync(u, m.ModuleId, "Export") });
                }

                actionList.Add(new ActionViewModel { Name = "" });

                if (ModuleState.PaneModuleIndex > 0)
                {
                    actionList.Add(new ActionViewModel { Icon = Icons.DataTransferUpload, Name = "Move To Top", Action = async (s, m) => await MoveTop(s, m) });
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
            return EditUrl(moduleId, import);
        }

        protected async Task ModuleAction(ActionViewModel action)
        {
            if (PageState.EditMode && UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, ModuleState.PermissionList))
            {
                PageModule pagemodule = await PageModuleService.GetPageModuleAsync(ModuleState.PageModuleId);

                string url = NavigateUrl(true);

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
            url = EditUrl(pagemodule.ModuleId, "Settings");
            return url;
        }

        private async Task<string> Publish(string url, PageModule pagemodule)
        {
            var permissions = pagemodule.Module.PermissionList;
            if (!permissions.Any(item => item.PermissionName == PermissionNames.View && item.RoleName == RoleNames.Everyone))
            {
                permissions.Add(new Permission(ModuleState.SiteId, EntityNames.Page, pagemodule.PageId, PermissionNames.View, RoleNames.Everyone, null, true));
            }
            if (!permissions.Any(item => item.PermissionName == PermissionNames.View && item.RoleName == RoleNames.Registered))
            {
                permissions.Add(new Permission(ModuleState.SiteId, EntityNames.Page, pagemodule.PageId, PermissionNames.View, RoleNames.Registered, null, true));
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
