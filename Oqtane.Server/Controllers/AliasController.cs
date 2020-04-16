using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using System;
using System.Net;
using System.Globalization;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class AliasController : Controller
    {
        private readonly IAliasRepository _aliases;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public AliasController(IAliasRepository aliases, ISyncManager syncManager, ILogManager logger)
        {
            _aliases = aliases;
            _syncManager = syncManager;
            _logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.AdminRole)]
        public IEnumerable<Alias> Get()
        {
            return _aliases.GetAliases();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Alias Get(int id)
        {
            return _aliases.GetAlias(id);
        }

        // GET api/<controller>/name/localhost:12345?lastsyncdate=yyyyMMddHHmmssfff
        [HttpGet("name/{name}")]
        public Alias Get(string name, string lastsyncdate)
        {
            name = WebUtility.UrlDecode(name);
            List<Alias> aliases = _aliases.GetAliases().ToList();
            Alias alias = null;
            alias = aliases.FirstOrDefault(item => item.Name == name);
            if (name != null && (alias == null && name.Contains("/")))
            {
                // lookup alias without folder name
                alias = aliases.Find(item => item.Name == name.Substring(0, name.IndexOf("/", StringComparison.Ordinal)));
            }
            if (alias == null && aliases.Count > 0)
            {
                // use first alias if name does not exist
                alias = aliases.FirstOrDefault();
            }

            // get sync events
            if (alias != null)
            {
                alias.SyncDate = DateTime.UtcNow;
                alias.SyncEvents = _syncManager.GetSyncEvents(DateTime.ParseExact(lastsyncdate, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture));
            }
            return alias;
        }
        
        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Alias Post([FromBody] Alias alias)
        {
            if (ModelState.IsValid)
            {
                alias = _aliases.AddAlias(alias);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Alias Added {Alias}", alias);
            }
            return alias;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Alias Put(int id, [FromBody] Alias alias)
        {
            if (ModelState.IsValid)
            {
                alias = _aliases.UpdateAlias(alias);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Alias Updated {Alias}", alias);
            }
            return alias;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            _aliases.DeleteAlias(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Alias Deleted {AliasId}", id);
        }
    }
}
