using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using [Owner].Module.[Module].Services;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;

namespace [Owner].Module.[Module].Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class [Module]Controller : ModuleControllerBase
    {
        private readonly I[Module]Service _[Module]Service;

        public [Module]Controller(I[Module]Service [Module]Service, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _[Module]Service = [Module]Service;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.[Module]>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _[Module]Service.Get[Module]sAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized [Module] Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.[Module]> Get(int id, int moduleid)
        {
            Models.[Module] [Module] = await _[Module]Service.Get[Module]Async(id, moduleid);
            if ([Module] != null && IsAuthorizedEntityId(EntityNames.Module, [Module].ModuleId))
            {
                return [Module];
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized [Module] Get Attempt {[Module]Id} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.[Module]> Post([FromBody] Models.[Module] [Module])
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, [Module].ModuleId))
            {
                [Module] = await _[Module]Service.Add[Module]Async([Module]);
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
        public async Task<Models.[Module]> Put(int id, [FromBody] Models.[Module] [Module])
        {
            if (ModelState.IsValid && [Module].[Module]Id == id && IsAuthorizedEntityId(EntityNames.Module, [Module].ModuleId))
            {
                [Module] = await _[Module]Service.Update[Module]Async([Module]);
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
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            Models.[Module] [Module] = await _[Module]Service.Get[Module]Async(id, moduleid);
            if ([Module] != null && IsAuthorizedEntityId(EntityNames.Module, [Module].ModuleId))
            {
                await _[Module]Service.Delete[Module]Async(id, [Module].ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized [Module] Delete Attempt {[Module]Id} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
