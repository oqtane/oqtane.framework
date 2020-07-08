using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using [Owner].[Module]s.Models;
using [Owner].[Module]s.Repository;

namespace [Owner].[Module]s.Controllers
{
    [Route("{site}/api/[controller]")]
    public class [Module]Controller : Controller
    {
        private readonly I[Module]Repository _[Module]s;
        private readonly ILogManager _logger;
        protected int _entityId = -1;

        public [Module]Controller(I[Module]Repository [Module]s, ILogManager logger, IHttpContextAccessor accessor)
        {
            _[Module]s = [Module]s;
            _logger = logger;

            if (accessor.HttpContext.Request.Query.ContainsKey("entityid"))
            {
                _entityId = int.Parse(accessor.HttpContext.Request.Query["entityid"]);
            }
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = "ViewModule")]
        public IEnumerable<[Module]> Get(string moduleid)
        {
            return _[Module]s.Get[Module]s(int.Parse(moduleid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Policy = "ViewModule")]
        public [Module] Get(int id)
        {
            [Module] [Module] = _[Module]s.Get[Module](id);
            if ([Module] != null && [Module].ModuleId != _entityId)
            {
                [Module] = null;
            }
            return [Module];
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = "EditModule")]
        public [Module] Post([FromBody] [Module] [Module])
        {
            if (ModelState.IsValid && [Module].ModuleId == _entityId)
            {
                [Module] = _[Module]s.Add[Module]([Module]);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "[Module] Added {[Module]}", [Module]);
            }
            return [Module];
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = "EditModule")]
        public [Module] Put(int id, [FromBody] [Module] [Module])
        {
            if (ModelState.IsValid && [Module].ModuleId == _entityId)
            {
                [Module] = _[Module]s.Update[Module]([Module]);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "[Module] Updated {[Module]}", [Module]);
            }
            return [Module];
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "EditModule")]
        public void Delete(int id)
        {
            [Module] [Module] = _[Module]s.Get[Module](id);
            if ([Module] != null && [Module].ModuleId == _entityId)
            {
                _[Module]s.Delete[Module](id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "[Module] Deleted {[Module]Id}", id);
            }
        }
    }
}
