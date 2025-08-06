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
using System.Globalization;

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
                return _visitors.GetVisitors(SiteId, DateTime.ParseExact(fromdate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal));
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
                var visitorCookieName = Constants.VisitorCookiePrefix + _alias.SiteId.ToString();
                authorized = (id == GetVisitorCookieId(Request.Cookies[visitorCookieName]));
            }

            var visitor = _visitors.GetVisitor(id);
            if (authorized && visitor != null && visitor.SiteId == _alias.SiteId)
            {
                return visitor;
            }
            else
            {
                if (visitor != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Visitor Get Attempt {VisitorId}", id);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        private int GetVisitorCookieId(string visitorCookie)
        {
            var visitorId = -1;
            if (visitorCookie != null)
            {
                // visitor cookies now contain the visitor id and an expiry date separated by a pipe symbol
                visitorCookie = (visitorCookie.Contains("|")) ? visitorCookie.Split('|')[0] : visitorCookie;
                visitorId = int.TryParse(visitorCookie, out int _visitorId) ? _visitorId : -1;
            }
            return visitorId;
        }
    }
}
