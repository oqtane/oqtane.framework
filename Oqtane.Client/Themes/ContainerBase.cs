using Microsoft.AspNetCore.Components;
using Oqtane.Models;

namespace Oqtane.Themes
{
    public abstract class ContainerBase : ThemeBase, IContainerControl
    {
        [CascadingParameter]
        protected Module ModuleState { get; set; }

    }
}
