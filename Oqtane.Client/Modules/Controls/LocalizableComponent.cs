using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Oqtane.Shared;

namespace Oqtane.Modules.Controls
{
    public class LocalizableComponent : ModuleControlBase
    {
        [Parameter]
        public string ResourceKey { get; set; }

        protected IStringLocalizer Localizer { get; private set; }

        protected override void OnParametersSet()
        {
            if (!String.IsNullOrEmpty(ResourceKey))
            {
                if (ModuleState?.ModuleType != null)
                {
                    var moduleType = Type.GetType(ModuleState.ModuleType);
                    var localizerTypeName = $"Microsoft.Extensions.Localization.IStringLocalizer`1[[{moduleType.AssemblyQualifiedName}]], Microsoft.Extensions.Localization.Abstractions";
                    var localizerType = Type.GetType(localizerTypeName);

                    // HACK: Use ServiceActivator instead of injecting IHttpContextAccessor, because HttpContext throws NRE in WebAssembly runtime
                    using (var scope = ServiceActivator.GetScope())
                    {
                        Localizer = (IStringLocalizer)scope.ServiceProvider.GetService(localizerType);
                    }
                }
            }
        }
    }
}
