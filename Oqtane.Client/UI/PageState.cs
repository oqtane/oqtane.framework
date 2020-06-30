using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.UI
{
    public class PageState
    {
        public Alias Alias { get; set; }
        public Site Site { get; set; }
        public List<Page> Pages { get; set; }
        public Page Page { get; set; }
        public User User { get; set; }
        public List<Module> Modules { get; set; }
        public Uri Uri { get; set; }
        public Dictionary<string, string> QueryString { get; set; }
        public string UrlParameters { get; set; }
        public int ModuleId { get; set; }
        public string Action { get; set; }
        public bool EditMode { get; set; }
        public DateTime LastSyncDate { get; set; }
        public Runtime Runtime { get; set; }
    }
}