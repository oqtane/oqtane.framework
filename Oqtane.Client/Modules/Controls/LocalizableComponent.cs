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
        private IStringLocalizer _resourceLocalizer;

        [Parameter]
        public string ResourceKey { get; set; }

        [Parameter]
        public string ResourceType { get; set; }

        protected bool IsLocalizable { get; private set; }

        protected IStringLocalizer T
        {
            get
            {
                if (_resourceLocalizer == null)
                {
                    using (var scope = ServiceActivator.GetScope())
                    {
                        var controlType = GetType();
                        var controlTypeName = controlType.Name;

                        // Remove `1 if the control is generic type
                        if (controlTypeName.EndsWith("`1"))
                        {
                            controlTypeName = controlTypeName.TrimEnd('`', '1');
                        }

                        var baseName = controlType.FullName[0..controlType.FullName.IndexOf(controlTypeName)].Substring("Oqtane.".Length);
                        var localizerFactory = scope.ServiceProvider.GetService<IStringLocalizerFactory>();

                        _resourceLocalizer = localizerFactory.Create(baseName + controlTypeName, controlType.Assembly.GetName().Name);
                    }
                }

                return _resourceLocalizer;
            }
        }

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

            if (String.IsNullOrEmpty(ResourceType))
            {
                ResourceType = ModuleState?.ModuleType;
            }

            if (!String.IsNullOrEmpty(ResourceKey) && !String.IsNullOrEmpty(ResourceType))
            {
                var moduleType = Type.GetType(ResourceType);
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
