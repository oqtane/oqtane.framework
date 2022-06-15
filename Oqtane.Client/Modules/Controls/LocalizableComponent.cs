using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Oqtane.Modules.Controls
{
    public class LocalizableComponent : ModuleControlBase
    {
        [Inject] public IStringLocalizerFactory LocalizerFactory { get; set; }

        private IStringLocalizer _localizer;

        [Parameter]
        public string ResourceKey { get; set; }

        [Parameter]
        public string ResourceType { get; set; }

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

            if (value == key || value == String.Empty)
            {
                // return default property value if key does not exist in resource file or value is empty
                return propertyValue;
            }
            else
            {
                // return localized value
                return value;
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
                _localizer = LocalizerFactory.Create(ResourceType);
                IsLocalizable = true;
            }
        }
    }
}
