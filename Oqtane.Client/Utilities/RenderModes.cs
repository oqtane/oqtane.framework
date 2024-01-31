using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

namespace Oqtane.Client.Utilities
{
    public static class RenderModes
    {
        public static IComponentRenderMode GetInteractiveRenderMode(string interactiveRenderMode, bool prerender)
        {
            switch (interactiveRenderMode)
            {
                case "InteractiveServer":
                    return new InteractiveServerRenderMode(prerender);
                case "InteractiveWebAssembly":
                    return new InteractiveWebAssemblyRenderMode(prerender);
                case "InteractiveAuto":
                    return new InteractiveAutoRenderMode(prerender);
            }
            return null;
        }
    }
}
