using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.UI
{
    public static class RenderMode
    {
        public static IComponentRenderMode GetInteractiveRenderMode(string interactiveRenderMode, bool prerender)
        {
            switch (interactiveRenderMode)
            {
                case RenderModes.InteractiveServer:
                    return new InteractiveServerRenderMode(prerender);
                case RenderModes.InteractiveWebAssembly:
                    return new InteractiveWebAssemblyRenderMode(prerender);
                case RenderModes.InteractiveAuto:
                    return new InteractiveAutoRenderMode(prerender);
            }
            return null;
        }
    }
}
