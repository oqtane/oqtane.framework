using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Net;
using System;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class VisitorController : Controller
    {
        private readonly IVisitorRepository _visitors;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public VisitorController(IVisitorRepository visitors, ILogManager logger, ITenantManager tenantManager)
        {
            _visitors = visitors;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x&fromdate=y
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<Visitor> Get(string siteid, string fromdate)
        {
            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                return _visitors.GetVisitors(SiteId, DateTime.Parse(fromdate));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Visitor Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Visitor Get(int id)
        {
            bool authorized = User.IsInRole(RoleNames.Admin);
            if (!authorized)
            {
                var visitorCookie = Constants.VisitorCookiePrefix + _alias.SiteId.ToString();
                if (int.TryParse(Request.Cookies[visitorCookie], out int visitorId))
                {
                    authorized = (visitorId == id);
                }
            }

            var visitor = _visitors.GetVisitor(id);
            if (authorized && visitor != null && visitor.SiteId == _alias.SiteId)
            {
                return visitor;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Visitor Get Attempt {VisitorId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }
    }
}
