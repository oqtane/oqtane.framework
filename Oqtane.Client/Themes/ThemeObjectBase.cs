using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Themes
{
    public class ThemeObjectBase : ComponentBase
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }
    }
}
