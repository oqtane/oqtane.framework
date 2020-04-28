using System;
using System.Collections.Generic;
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

        protected List<ActionViewModel> Actions;

        protected override void OnParametersSet()
        {
            Actions = GetActions();
        }

        protected virtual List<ActionViewModel> GetActions()
        {
            var actionList = new List<ActionViewModel>();
            if (PageState.EditMode && UserSecurity.IsAuthorized(PageState.User, PermissionNames.Edit, ModuleState.Permissions))
            {
                actionList.Add(new ActionViewModel {Name = "Manage Settings", Action = async (u, m) => await Settings(u, m)});

                if (ModuleState.ModuleDefinition != null && ModuleState.ModuleDefinition.ServerManagerType != "")
                {
                    actionList.Add(new ActionViewModel {Name = "Import Content", Action = async (u, m) => await EditUrlAsync(u, m.ModuleId, "Import")});
                    actionList.Add(new ActionViewModel {Name = "Export Content", Action = async (u, m) => await EditUrlAsync(u, m.ModuleId, "Export")});
                }

                actionList.Add(new ActionViewModel {Name = "Delete Module", Action = async (u, m) => await DeleteModule(u, m)});
                actionList.Add(new ActionViewModel {Name = ""});

                if (ModuleState.PaneModuleIndex > 0)
                {
                    actionList.Add(new ActionViewModel {Name = "Move To Top", Action = async (s, m) => await MoveTop(s, m)});
                }

                if (ModuleState.PaneModuleIndex > 0)
                {
                    actionList.Add(new ActionViewModel {Name = "Move Up", Action = async (s, m) => await MoveUp(s, m)});
                }

                if (ModuleState.PaneModuleIndex < (ModuleState.PaneModuleCount - 1))
                {
                    actionList.Add(new ActionViewModel {Name = "Move Down", Action = async (s, m) => await MoveDown(s, m)});
                }

                if (ModuleState.PaneModuleIndex < (ModuleState.PaneModuleCount - 1))
                {
                    actionList.Add(new ActionViewModel {Name = "Move To Bottom", Action = async (s, m) => await MoveBottom(s, m)});
                }

                foreach (string pane in PageState.Page.Panes.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (pane != ModuleState.Pane)
                    {
                        actionList.Add(new ActionViewModel {Name = "Move To " + pane + " Pane", Action = async (s, m) => await MoveToPane(s, pane, m)});
                    }
                }
            }

            return actionList;
        }

        private async Task<string> EditUrlAsync(string url, int moduleId, string import)
        {
            await Task.Yield();
            EditUrl(moduleId, import);
            return url;
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
            public string Name { set; get; }

            public Func<string, PageModule, Task<string>> Action { set; get; }
        }
    }
}
