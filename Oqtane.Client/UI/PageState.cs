using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Oqtane.Models;

namespace Oqtane.UI
{
    public class PageState
    {
        public Alias Alias { get; set; }
        public Site Site { get; set; }
        public Page Page { get; set; }
        public User User { get; set; }
        public Uri Uri { get; set; }
        public Dictionary<string, string> QueryString { get; set; }
        public string UrlParameters { get; set; }
        public int ModuleId { get; set; }
        public string Action { get; set; }
        public bool EditMode { get; set; }
        public DateTime LastSyncDate { get; set; }
        public Shared.Runtime Runtime { get; set; }
        public int VisitorId { get; set; }
        public string RemoteIPAddress { get; set; }
        public string ReturnUrl { get; set; }

        public List<Page> Pages
        {
            get { return Site.Pages.Where(item => !item.IsDeleted).ToList(); }
        }
        public List<Module> Modules
        {
            get { return Site.Modules.Where(item => !item.IsDeleted).ToList(); }
        }
        public List<Language> Languages
        {
            get { return Site.Languages; }
        }
    }
}
