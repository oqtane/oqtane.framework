using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using Oqtane.Models;

namespace Oqtane.Themes
{
    public class ContainerBase : ComponentBase, IContainerControl
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }

        [CascadingParameter]
        protected Module ModuleState { get; set; }

        public virtual string Name { get; set; }
    }
}
