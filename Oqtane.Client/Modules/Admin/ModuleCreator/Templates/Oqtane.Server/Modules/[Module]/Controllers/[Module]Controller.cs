using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models.[Module]s;
using Oqtane.Repository.[Module]s;
using Oqtane.Shared;
using System.Collections.Generic;
using Oqtane.Enums;
using Oqtane.Infrastructure.Interfaces;

namespace Oqtane.Controllers.[Module]s
{
    [Route("{site}/api/[controller]")]
    public class [Module]Controller : Controller
    {
        private readonly I[Module]Repository _[Module]s;
        private readonly ILogManager _logger;

        public [Module]Controller(I[Module]Repository [Module]s, ILogManager logger)
        {
            _[Module]s = [Module]s;
            _logger = logger;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Roles = Constants.RegisteredRole)]
        public IEnumerable<[Module]> Get(string moduleid)
        {
            return _[Module]s.Get[Module]s(int.Parse(moduleid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public [Module] Get(int id)
        {
            return _[Module]s.Get[Module](id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public [Module] Post([FromBody] [Module] [Module])
        {
            if (ModelState.IsValid)
            {
                [Module] = _[Module]s.Add[Module]([Module]);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "[Module] Added {[Module]}", [Module]);
            }
            return [Module];
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public [Module] Put(int id, [FromBody] [Module] [Module])
        {
            if (ModelState.IsValid)
            {
                [Module] = _[Module]s.Update[Module]([Module]);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "[Module] Updated {[Module]}", [Module]);
            }
            return [Module];
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            _[Module]s.Delete[Module](id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "[Module] Deleted {[Module]Id}", id);
        }
    }
}
