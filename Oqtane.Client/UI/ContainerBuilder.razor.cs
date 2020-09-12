using Microsoft.AspNetCore.Components;
using Oqtane.Models;
using Oqtane.Shared;
using System;

namespace Oqtane.UI
{
    public partial class ContainerBuilder
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }

        [Parameter]
        public Module Module { get; set; }

        RenderFragment DynamicComponent { get; set; }

        protected override void OnParametersSet()
        {
            var container = Module.ContainerType;
            if (PageState.ModuleId != -1 && Module.UseAdminContainer)
            {
                container = Constants.DefaultAdminContainer;
            }

            DynamicComponent = builder =>
            {
                var containerType = Type.GetType(container);
                if (containerType == null)
                {
                    // fallback
                    containerType = Type.GetType(Constants.DefaultContainer);
                }
                builder.OpenComponent(0, containerType);
                builder.CloseComponent();
            };
        }
    }
}
