using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using System.Linq;
using System;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class AliasController : Controller
    {
        private readonly IAliasRepository Aliases;
        private readonly ILogManager logger;

        public AliasController(IAliasRepository Aliases, ILogManager logger)
        {
            this.Aliases = Aliases;
            this.logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.AdminRole)]
        public IEnumerable<Alias> Get()
        {
            return Aliases.GetAliases();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Alias Get(int id)
        {
            return Aliases.GetAlias(id);
        }

        // GET api/<controller>/name/localhost:12345
        [HttpGet("name/{name}")]
        public Alias Get(string name)
        {
            List<Alias> aliases = Aliases.GetAliases().ToList();
            Alias alias = null;
            alias = aliases.Where(item => item.Name == name).FirstOrDefault();
            if (alias == null && name.Contains("/"))
            {
                // lookup alias without folder name
                alias = aliases.Where(item => item.Name == name.Substring(name.IndexOf("/") + 1)).FirstOrDefault();
            }
            if (alias == null && aliases.Count > 0)
            {
                // use first alias if name does not exist
                alias = aliases.FirstOrDefault();
            }
            return alias; 
        }
        
        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Alias Post([FromBody] Alias Alias)
        {
            if (ModelState.IsValid)
            {
                Alias = Aliases.AddAlias(Alias);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Alias Added {Alias}", Alias);
            }
            return Alias;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Alias Put(int id, [FromBody] Alias Alias)
        {
            if (ModelState.IsValid)
            {
                Alias = Aliases.UpdateAlias(Alias);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Alias Updated {Alias}", Alias);
            }
            return Alias;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            Aliases.DeleteAlias(id);
            logger.Log(LogLevel.Information, this, LogFunction.Delete, "Alias Deleted {AliasId}", id);
        }
    }
}
