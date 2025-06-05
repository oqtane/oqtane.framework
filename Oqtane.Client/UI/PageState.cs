using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.UI
{
    public class PageState
    {
        public Alias Alias { get; set; }
        public Site Site { get; set; }
        public Page Page { get; set; }
        public List<Module> Modules { get; set; }
        public User User { get; set; }
        public Uri Uri { get; set; }
        public Route Route { get; set; }
        public Dictionary<string, string> QueryString { get; set; }
        public string UrlParameters { get; set; }
        public int ModuleId { get; set; }
        public string Action { get; set; }
        public bool EditMode { get; set; }
        public DateTime LastSyncDate { get; set; }
        public string RenderMode { get; set; }
        public Shared.Runtime Runtime { get; set; }
        public int VisitorId { get; set; }
        public string RemoteIPAddress { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsInternalNavigation { get; set; }
        public Guid RenderId { get; set; }
        public bool Refresh {  get; set; }
        public bool AllowCookies { get; set; }

        public List<Page> Pages
        {
            get { return Site?.Pages; }
        }
        public List<Language> Languages
        {
            get { return Site?.Languages; }
        }

        public PageState Clone()
        {
            return new PageState
            {
                Alias = Alias,
                Site = Site,
                Page = Page,
                Modules = Modules,
                User = User,
                Uri = Uri,
                Route = Route,
                QueryString = QueryString,
                UrlParameters = UrlParameters,
                ModuleId = ModuleId,
                Action = Action,
                EditMode = EditMode,
                LastSyncDate = LastSyncDate,
                RenderMode = RenderMode,
                Runtime = Runtime,
                VisitorId = VisitorId,
                RemoteIPAddress = RemoteIPAddress,
                ReturnUrl = ReturnUrl,
                IsInternalNavigation = IsInternalNavigation,
                RenderId = RenderId,
                Refresh = Refresh,
                AllowCookies = AllowCookies
            };
        }
    }
}
