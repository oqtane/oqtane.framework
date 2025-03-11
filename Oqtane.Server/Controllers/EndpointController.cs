using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class EndpointController : Controller
    {
        private readonly IEnumerable<EndpointDataSource> _endpointSources;

        public EndpointController(IEnumerable<EndpointDataSource> endpointSources)
        {
            _endpointSources = endpointSources;
        }

        // GET api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public ActionResult Get()
        {
            var endpoints = _endpointSources
                .SelectMany(item => item.Endpoints)
                .OfType<RouteEndpoint>();

            var output = endpoints.Select(
                item =>
                {
                    var controller = item.Metadata
                        .OfType<ControllerActionDescriptor>()
                        .FirstOrDefault();
                    var action = controller != null
                        ? $"{controller.ControllerName}.{controller.ActionName}"
                        : null;
                    var controllerMethod = controller != null
                        ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}"
                        : null;
                    return new
                    {
                        Method = item.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
                        Route = $"/{item.RoutePattern.RawText.TrimStart('/')}",
                        Action = action,
                        ControllerMethod = controllerMethod
                    };
                }
            ).OrderBy(item => item.Route);

            return Json(output);
        }
    }
}
