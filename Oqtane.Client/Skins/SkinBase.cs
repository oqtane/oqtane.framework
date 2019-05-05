using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Skins
{
    public class SkinBase : ComponentBase, ISkinControl
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }
        public virtual string Name { get; set; }
        public virtual string Panes { get; set; }
    }
}
