using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System;

namespace Oqtane.UI
{
    public partial class PaneLayout
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }

        RenderFragment DynamicComponent { get; set; }

        protected override void OnParametersSet()
        {
            DynamicComponent = builder =>
            {
                var layoutType = Type.GetType(PageState.Page.LayoutType);
                if (layoutType == null)
                {
                    // fallback
                    layoutType = Type.GetType(Constants.DefaultLayout);
                }
                builder.OpenComponent(0, layoutType);
                builder.CloseComponent();
            };
        }
    }
}
