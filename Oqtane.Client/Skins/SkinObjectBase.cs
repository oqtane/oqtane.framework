using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Skins
{
    public class SkinObjectBase : ComponentBase
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }
    }
}
