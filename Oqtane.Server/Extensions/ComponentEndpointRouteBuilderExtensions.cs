#nullable enable
using Oqtane.Components;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Routing;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Antiforgery;

namespace OqtaneSSR.Extensions
{
    public static class ComponentEndpointRouteBuilderExtensions
    {
        public static void MapFallback(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            endpoints.MapFallback((ctx) =>
            {
                var invoker = ctx.RequestServices.GetRequiredService<IRazorComponentEndpointInvoker>();
                return invoker.Render(ctx);
            })
            .Add(routeEndpointBuilder =>
            {
                routeEndpointBuilder.Metadata.Add(new RootComponentMetadata(typeof(App)));
                routeEndpointBuilder.Metadata.Add(new ComponentTypeMetadata(typeof(App)));
                routeEndpointBuilder.Metadata.Add(new RequireAntiforgeryTokenAttribute());
            });
        }
    }
}
