using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.UI
{
    public static class InteractiveRenderMode
    {
        public static IComponentRenderMode GetInteractiveRenderMode(string runtime, bool prerender)
        {
            switch (runtime)
            {
                case Runtimes.Server:
                    return new InteractiveServerRenderMode(prerender: prerender);
                case Runtimes.WebAssembly:
                    return new InteractiveWebAssemblyRenderMode(prerender: prerender);
                case Runtimes.Auto:
                    return new InteractiveAutoRenderMode(prerender: prerender);
            }
            return new InteractiveServerRenderMode(prerender: prerender);
        }
    }
}
