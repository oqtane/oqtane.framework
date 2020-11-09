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
            var key = $"{ResourceKey}.{name}";

            // TODO: we should have a ShowMissingResourceKeys option which developers/translators can enable to find missing translations which would display the key rather than the name    
            if (!IsLocalizable)
            {
                return name;
            }
 
            return _localizer?[key] ?? name;
        }

        protected override void OnParametersSet()
        {
            if (!String.IsNullOrEmpty(ResourceKey))
            {
                if (ModuleState?.ModuleType != null)
                {
                    var moduleType = Type.GetType(ModuleState.ModuleType);
                    if (moduleType != null)
                    {
                        using (var scope = ServiceActivator.GetScope())
                        {
                            var localizerFactory = scope.ServiceProvider.GetService<IStringLocalizerFactory>();
                            _localizer = localizerFactory.Create(moduleType);
                        }
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
