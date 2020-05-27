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
using Microsoft.AspNetCore.Http;

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class AliasController : Controller
    {
        private readonly IAliasRepository _aliases;
        private readonly IHttpContextAccessor _accessor;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public AliasController(IAliasRepository aliases, IHttpContextAccessor accessor, ISyncManager syncManager, ILogManager logger)
        {
            _aliases = aliases;
            _accessor = accessor;
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

        // GET api/<controller>/name/xxx?sync=yyyyMMddHHmmssfff
        [HttpGet("name/{**name}")]
        public Alias Get(string name, string sync)
        {
            List<Alias> aliases = _aliases.GetAliases().ToList(); // cached
            Alias alias = null;
            if (_accessor.HttpContext != null)
            {
                name = (name == "~") ? "" : name;
                name = _accessor.HttpContext.Request.Host.Value + "/" + WebUtility.UrlDecode(name);
                var segments = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                // iterate segments in reverse order
                for (int i = segments.Length; i > 0; i--)
                {
                    name = string.Join("/", segments, 0, i);
                    alias = aliases.Find(item => item.Name == name);
                    if (alias != null)
                    {
                        break; // found a matching alias
                    }
                }
            }
            if (alias == null && aliases.Any())
            {
                // use first alias if name does not exist
                alias = aliases.FirstOrDefault();
            }

            // get sync events
            if (alias != null)
            {
                alias.SyncDate = DateTime.UtcNow;
                alias.SyncEvents = _syncManager.GetSyncEvents(alias.TenantId, DateTime.ParseExact(sync, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture));
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
