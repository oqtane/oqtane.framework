using System.Collections.Generic;
using System;
using Oqtane.Models;
using Oqtane.UI;

namespace Oqtane.Themes.Controls
{
    public class ControlPanelPageState
    {
        public Alias Alias { get; set; }
        public Site Site { get; set; }
        public Page Page { get; set; }
        public User User { get; set; }
        public Uri Uri { get; set; }
        public Route Route { get; set; }
        public string RenderMode { get; set; }
        public Shared.Runtime Runtime { get; set; }
    }
}
