using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Oqtane.Infrastructure;
using System.Collections.Generic;
using System;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    public class ModuleControllerBase : Controller
    {
        protected readonly ILogManager _logger;

        // parameters for policy authorization and validation
        protected Dictionary<string, int> _authEntityId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        protected int _entityId = -1; // legacy support

        public ModuleControllerBase(ILogManager logger, IHttpContextAccessor accessor)
        {
            _logger = logger;

            // populate policy authorization dictionary from querystring
            int value;
            foreach (var param in accessor.HttpContext.Request.Query)
            {
                if (param.Key.StartsWith("auth") && param.Key.EndsWith("id") && int.TryParse(param.Value, out value))
                {
                    _authEntityId.Add(param.Key.Substring(4, param.Key.Length - 6), value);
                }
            }

            // legacy support
            if (_authEntityId.Count == 0 && accessor.HttpContext.Request.Query.ContainsKey("entityid"))
            {
                _entityId = int.Parse(accessor.HttpContext.Request.Query["entityid"]);
            }

        }

        protected int AuthEntityId(string entityname)
        {
            if (_authEntityId.ContainsKey(entityname))
            {
                return _authEntityId[entityname];
            }
            else
            {
                return -1;
            }
        }

        protected bool IsAuthorizedEntityId(string entityname, int entityid)
        {
            return (entityid == AuthEntityId(entityname)) || User.IsInRole(RoleNames.Host);
        }
    }
}
