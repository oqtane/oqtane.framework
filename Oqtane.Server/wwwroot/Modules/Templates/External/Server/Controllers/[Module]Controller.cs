using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using [Owner].Module.[Module].Repository;
using Oqtane.Controllers;
using System.Net;

namespace [Owner].Module.[Module].Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class [Module]Controller : ModuleControllerBase
    {
        private readonly I[Module]Repository _[Module]Repository;

        public [Module]Controller(I[Module]Repository [Module]Repository, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _[Module]Repository = [Module]Repository;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public IEnumerable<Models.[Module]> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return _[Module]Repository.Get[Module]s(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized [Module] Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public Models.[Module] Get(int id)
        {
            Models.[Module] [Module] = _[Module]Repository.Get[Module](id);
            if ([Module] != null && IsAuthorizedEntityId(EntityNames.Module, [Module].ModuleId))
            {
                return [Module];
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized [Module] Get Attempt {[Module]Id}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Models.[Module] Post([FromBody] Models.[Module] [Module])
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, [Module].ModuleId))
            {
                [Module] = _[Module]Repository.Add[Module]([Module]);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "[Module] Added {[Module]}", [Module]);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized [Module] Post Attempt {[Module]}", [Module]);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                [Module] = null;
            }
            return [Module];
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Models.[Module] Put(int id, [FromBody] Models.[Module] [Module])
        {
            if (ModelState.IsValid && [Module].[Module]Id == id && IsAuthorizedEntityId(EntityNames.Module, [Module].ModuleId) && _[Module]Repository.Get[Module]([Module].[Module]Id, false) != null)
            {
                [Module] = _[Module]Repository.Update[Module]([Module]);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "[Module] Updated {[Module]}", [Module]);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized [Module] Put Attempt {[Module]}", [Module]);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                [Module] = null;
            }
            return [Module];
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public void Delete(int id)
        {
            Models.[Module] [Module] = _[Module]Repository.Get[Module](id);
            if ([Module] != null && IsAuthorizedEntityId(EntityNames.Module, [Module].ModuleId))
            {
                _[Module]Repository.Delete[Module](id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "[Module] Deleted {[Module]Id}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized [Module] Delete Attempt {[Module]Id}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
