using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using [Owner].[Module].Models;
using [Owner].[Module].Repository;

namespace [Owner].[Module].Controllers
{
    [Route(ControllerRoutes.Default)]
    public class [Module]Controller : Controller
    {
        private readonly I[Module]Repository _[Module]Repository;
        private readonly ILogManager _logger;
        protected int _entityId = -1;

        public [Module]Controller(I[Module]Repository [Module]Repository, ILogManager logger, IHttpContextAccessor accessor)
        {
            _[Module]Repository = [Module]Repository;
            _logger = logger;

            if (accessor.HttpContext.Request.Query.ContainsKey("entityid"))
            {
                _entityId = int.Parse(accessor.HttpContext.Request.Query["entityid"]);
            }
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public IEnumerable<Models.[Module]> Get(string moduleid)
        {
            return _[Module]Repository.Get[Module]s(int.Parse(moduleid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public Models.[Module] Get(int id)
        {
            Models.[Module] [Module] = _[Module]Repository.Get[Module](id);
            if ([Module] != null && [Module].ModuleId != _entityId)
            {
                [Module] = null;
            }
            return [Module];
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Models.[Module] Post([FromBody] Models.[Module] [Module])
        {
            if (ModelState.IsValid && [Module].ModuleId == _entityId)
            {
                [Module] = _[Module]Repository.Add[Module]([Module]);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "[Module] Added {[Module]}", [Module]);
            }
            return [Module];
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Models.[Module] Put(int id, [FromBody] Models.[Module] [Module])
        {
            if (ModelState.IsValid && [Module].ModuleId == _entityId)
            {
                [Module] = _[Module]Repository.Update[Module]([Module]);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "[Module] Updated {[Module]}", [Module]);
            }
            return [Module];
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public void Delete(int id)
        {
            Models.[Module] [Module] = _[Module]Repository.Get[Module](id);
            if ([Module] != null && [Module].ModuleId == _entityId)
            {
                _[Module]Repository.Delete[Module](id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "[Module] Deleted {[Module]Id}", id);
            }
        }
    }
}
