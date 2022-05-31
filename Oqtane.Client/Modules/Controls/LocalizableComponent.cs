using System;
using System.Text;
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
                        var assemblyName = controlType.Assembly.GetName().Name;

                        // Trim "`1" suffix (for generic type) to make it consistent like normal control types
                        if (controlTypeName.EndsWith("`1"))
                        {
                            controlTypeName = controlTypeName.TrimEnd('`', '1');
                        }

                        controlTypeName = controlType.FullName[..(controlType.FullName.IndexOf(controlTypeName) + controlTypeName.Length)];

                        var baseName = GetBaseName(controlTypeName, assemblyName);
                        var localizerFactory = scope.ServiceProvider.GetService<IStringLocalizerFactory>();

                        _resourceLocalizer = localizerFactory.Create(baseName, assemblyName);
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

        private static string GetBaseName(string typeName, string assemblyName)
        {
            var baseName = new StringBuilder(typeName);

            var tokens = assemblyName.Split(".");

            foreach (var token in tokens)
            {
                var index = baseName.ToString().IndexOf(token);

                if (index == -1)
                {
                    continue;
                }

                baseName.Remove(index, token.Length + 1);
            }

            return baseName.ToString();
        }
    }
}
