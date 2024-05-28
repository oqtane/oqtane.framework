using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Oqtane.Models;
using Oqtane.Services;

namespace Oqtane.UI
{
    public class PageState
    {
        private readonly ISiteService _siteService;

        public PageState(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public Alias Alias { get; set; }
        public int SiteId { get; set; }

        [JsonIgnore]
        public Site Site
        {
            get
            {
                return _siteService.GetSiteAsync(SiteId).Result;
            }
        }
        public Page Page { get; set; }
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

        [JsonIgnore]
        public List<Page> Pages
        {
            get { return Site.Pages; }
        }

        [JsonIgnore]
        public List<Module> Modules
        {
            get { return Site.Modules; }
        }

        [JsonIgnore]
        public List<Language> Languages
        {
            get { return Site.Languages; }
        }
    }
}
