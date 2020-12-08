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

        protected string Localize(string name) => _localizer?[name] ?? name;

        protected string Localize(string propertyName, string propertyValue)
        {
            if (!IsLocalizable)
            {
                return propertyValue;
            }

            var key = $"{ResourceKey}.{propertyName}";
            var value = Localize(key);

            if (value == key)
            {
                // Returns default property value (English version) instead of ResourceKey.PropertyName
                return propertyValue;
            }
            else
            {
                if (value == String.Empty)
                {
                    // Returns default property value (English version)
                    return propertyValue;
                }
                else
                {
                    return value;
                }
            }
        }

        protected override void OnParametersSet()
        {
            IsLocalizable = false;

            if (!String.IsNullOrEmpty(ResourceKey) && ModuleState?.ModuleType != null)
            {
                var moduleType = Type.GetType(ModuleState.ModuleType);
                if (moduleType != null)
                {
                    using (var scope = ServiceActivator.GetScope())
                    {
                        var localizerFactory = scope.ServiceProvider.GetService<IStringLocalizerFactory>();
                        _localizer = localizerFactory.Create(moduleType);

                        IsLocalizable = true;
                    }
                }
            }
        }
    }
}
