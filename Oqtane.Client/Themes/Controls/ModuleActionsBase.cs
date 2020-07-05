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
            if (PageState.EditMode && UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, ModuleState.Permissions))
            {
                actionList.Add(new ActionViewModel {Icon = Icons.Cog, Name = "Manage Settings", Action = async (u, m) => await Settings(u, m)});
                
                if (UserSecurity.GetPermissionStrings(ModuleState.Permissions).FirstOrDefault(item => item.PermissionName == PermissionNames.View).Permissions.Split(';').Contains(Constants.AllUsersRole))
                {
                    actionList.Add(new ActionViewModel {Icon=Icons.CircleX, Name = "Unpublish Module", Action = async (s, m) => await Unpublish(s, m) });
                }
                else
                {
                    actionList.Add(new ActionViewModel {Icon=Icons.CircleCheck, Name = "Publish Module", Action = async (s, m) => await Publish(s, m) });
                }
                actionList.Add(new ActionViewModel {Icon=Icons.Trash, Name = "Delete Module", Action = async (u, m) => await DeleteModule(u, m) });

                if (ModuleState.ModuleDefinition != null && ModuleState.ModuleDefinition.ServerManagerType != "")
                {
                    actionList.Add(new ActionViewModel { Name = "" });
                    actionList.Add(new ActionViewModel {Icon=Icons.CloudUpload, Name = "Import Content", Action = async (u, m) => await EditUrlAsync(u, m.ModuleId, "Import")});
                    actionList.Add(new ActionViewModel {Icon = Icons.CloudDownload, Name = "Export Content", Action = async (u, m) => await EditUrlAsync(u, m.ModuleId, "Export")});
                }

                actionList.Add(new ActionViewModel {Name = ""});

                if (ModuleState.PaneModuleIndex > 0)
                {
                    actionList.Add(new ActionViewModel {Icon = Icons.DataTransferUpload ,Name = "Move To Top", Action = async (s, m) => await MoveTop(s, m)});
                }

                if (ModuleState.PaneModuleIndex > 0)
                {
                    actionList.Add(new ActionViewModel {Icon = Icons.ArrowThickTop, Name = "Move Up", Action = async (s, m) => await MoveUp(s, m)});
                }

                if (ModuleState.PaneModuleIndex < (ModuleState.PaneModuleCount - 1))
                {
                    actionList.Add(new ActionViewModel {Icon = Icons.ArrowThickBottom, Name = "Move Down", Action = async (s, m) => await MoveDown(s, m)});
                }

                if (ModuleState.PaneModuleIndex < (ModuleState.PaneModuleCount - 1))
                {
                    actionList.Add(new ActionViewModel {Icon = Icons.DataTransferDownload, Name = "Move To Bottom", Action = async (s, m) => await MoveBottom(s, m)});
                }

                foreach (string pane in PageState.Page.Panes)
                {
                    if (pane != ModuleState.Pane)
                    {
                        actionList.Add(new ActionViewModel {Icon = Icons.AccountLogin, Name = "Move To " + pane + " Pane", Action = async (s, m) => await MoveToPane(s, pane, m)});
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
            if (PageState.EditMode && UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, ModuleState.Permissions))
            {
                PageModule pagemodule = await PageModuleService.GetPageModuleAsync(ModuleState.PageModuleId);

                string url = NavigateUrl();

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

        private async Task<string> Publish(string s, PageModule pagemodule)
        {
            var permissions = UserSecurity.GetPermissionStrings(pagemodule.Module.Permissions);
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
            pagemodule.Module.Permissions = UserSecurity.SetPermissionStrings(permissions);
            await ModuleService.UpdateModuleAsync(pagemodule.Module);
            return NavigateUrl(s, "reload");
        }

        private async Task<string> Unpublish(string s, PageModule pagemodule)
        {
            var permissions = UserSecurity.GetPermissionStrings(pagemodule.Module.Permissions);
            foreach (var permissionstring in permissions)
            {
                if (permissionstring.PermissionName == PermissionNames.View)
                {
                    List<string> ids = permissionstring.Permissions.Split(';').ToList();
                    ids.Remove(Constants.AllUsersRole);
                    ids.Remove(Constants.RegisteredRole);
                    permissionstring.Permissions = string.Join(";", ids.ToArray());
                }
            }
            pagemodule.Module.Permissions = UserSecurity.SetPermissionStrings(permissions);
            await ModuleService.UpdateModuleAsync(pagemodule.Module);
            return NavigateUrl(s, "reload");
        }

        private async Task<string> MoveTop(string s, PageModule pagemodule)
        {
            pagemodule.Order = 0;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return s;
        }

        private async Task<string> MoveBottom(string s, PageModule pagemodule)
        {
            pagemodule.Order = int.MaxValue;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return s;
        }

        private async Task<string> MoveUp(string s, PageModule pagemodule)
        {
            pagemodule.Order -= 3;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return s;
        }

        private async Task<string> MoveDown(string s, PageModule pagemodule)
        {
            pagemodule.Order += 3;
            await PageModuleService.UpdatePageModuleAsync(pagemodule);
            await PageModuleService.UpdatePageModuleOrderAsync(pagemodule.PageId, pagemodule.Pane);
            return s;
        }

        public class ActionViewModel
        {
            public string Icon { get; set; }
            public string Name { set; get; }
            public Func<string, PageModule, Task<string>> Action { set; get; }
        }
    }
}
