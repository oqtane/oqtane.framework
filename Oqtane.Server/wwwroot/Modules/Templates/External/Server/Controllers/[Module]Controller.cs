using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using [Owner].[Module].Repository;
using Oqtane.Controllers;

namespace [Owner].[Module].Controllers
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
            if (int.Parse(moduleid) == _entityId)
            {
                return _[Module]Repository.Get[Module]s(int.Parse(moduleid));
            }
            else
            {
                return null;
            }
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
