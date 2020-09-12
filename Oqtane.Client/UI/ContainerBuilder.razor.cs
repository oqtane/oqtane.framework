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

        public Module ModuleState { get; private set; }

        RenderFragment DynamicComponent { get; set; }

        protected override void OnParametersSet()
        {
            ModuleState = Module; // passed in from Pane component

            var container = ModuleState.ContainerType;
            if (PageState.ModuleId != -1 && ModuleState.UseAdminContainer)
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
