using Microsoft.AspNetCore.Components;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Modules.Controls;
using Oqtane.Shared;
using System;
using System.Linq;


namespace Oqtane.UI
{
    public partial class ModuleInstance
    {
        [CascadingParameter]
        protected PageState PageState { get; set; }

        [CascadingParameter]
        private Module ModuleState { get; set; }

        public ModuleMessage ModuleMessage { get; set; }

        public RenderFragment DynamicComponent { get; set; }

        public string Message { get; private set; }

        public bool Progressindicator { get; private set; } = false;

        protected override void OnParametersSet()
        {
            DynamicComponent = builder =>
            {
                string typename = ModuleState.ModuleType;
                // check for core module actions component
                if (Constants.DefaultModuleActions.Contains(PageState.Action))
                {
                    typename = Constants.DefaultModuleActionsTemplate.Replace(Constants.ActionToken, PageState.Action);
                }

                Type moduleType = null;
                if (typename != null)
                {
                    moduleType = Type.GetType(typename);

                    if (moduleType != null)
                    {
                        builder.OpenComponent(0, moduleType);
                        builder.CloseComponent();
                    }
                    else
                    {
                        // module does not exist with typename specified
                        Message = "Module Does Not Have A Component Named " + Utilities.GetTypeNameLastSegment(typename, 0) + ".razor";
                    }
                }
                else
                {
                    Message = "Something is wrong with moduletype";
                }
            };
        }

        public void AddModuleMessage(string message, MessageType type)
        {
            Progressindicator = false;
            StateHasChanged();
            ModuleMessage.SetModuleMessage(message, type);
        }

        public void ShowProgressIndicator()
        {
            Progressindicator = true;
            StateHasChanged();
        }

        public void HideProgressIndicator()
        {
            Progressindicator = false;
            StateHasChanged();
        }
    }
}
