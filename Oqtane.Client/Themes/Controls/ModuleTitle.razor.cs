using Oqtane.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Oqtane.Themes.Controls
{
    public partial class ModuleTitle : ContainerBase
    {
        public string Title { get; private set; }

        protected override Task OnParametersSetAsync()
        {
            Title = ModuleState.Title;
            // check for core module actions component
            if (Constants.DefaultModuleActions.Contains(PageState.Action))
            {
                Title = PageState.Action;
            }
            return Task.CompletedTask;
        }
    }
}
