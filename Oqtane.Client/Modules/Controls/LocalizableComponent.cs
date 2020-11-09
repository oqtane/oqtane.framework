using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Oqtane.Shared;

namespace Oqtane.Modules.Controls
{
    public class LocalizableComponent : ModuleControlBase
    {
        private IStringLocalizer _localizer;

        [Parameter]
        public string ResourceKey { get; set; }

        protected bool IsLocalizable { get; private set; }

        protected string Localize(string name)
        {
            if (!IsLocalizable)
            {
                return name;
            }

            var key = $"{ResourceKey}.{name}";
 
            return _localizer?[key] ?? key;
        }

        protected override void OnParametersSet()
        {
            if (!String.IsNullOrEmpty(ResourceKey))
            {
                if (ModuleState?.ModuleType != null)
                {
                    var moduleType = Type.GetType(ModuleState.ModuleType);

                    // HACK: Use ServiceActivator instead of injecting IHttpContextAccessor, because HttpContext throws NRE in WebAssembly runtime
                    using (var scope = ServiceActivator.GetScope())
                    {
                        var localizerFactory = scope.ServiceProvider.GetService<IStringLocalizerFactory>();
                        _localizer = localizerFactory.Create(moduleType);
                    }
                }

                IsLocalizable = true;
            }
            else
            {
                IsLocalizable = false;
            }
        }
    }
}
