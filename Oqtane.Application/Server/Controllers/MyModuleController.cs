using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Application.Services;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;

namespace Oqtane.Application.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MyModuleController : ModuleControllerBase
    {
        private readonly IMyModuleService _MyModuleService;

        public MyModuleController(IMyModuleService MyModuleService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _MyModuleService = MyModuleService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.MyModule>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _MyModuleService.GetMyModulesAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.MyModule> Get(int id, int moduleid)
        {
            Models.MyModule MyModule = await _MyModuleService.GetMyModuleAsync(id, moduleid);
            if (MyModule != null && IsAuthorizedEntityId(EntityNames.Module, MyModule.ModuleId))
            {
                return MyModule;
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Get Attempt {MyModuleId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.MyModule> Post([FromBody] Models.MyModule MyModule)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, MyModule.ModuleId))
            {
                MyModule = await _MyModuleService.AddMyModuleAsync(MyModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Post Attempt {MyModule}", MyModule);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                MyModule = null;
            }
            return MyModule;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.MyModule> Put(int id, [FromBody] Models.MyModule MyModule)
        {
            if (ModelState.IsValid && MyModule.MyModuleId == id && IsAuthorizedEntityId(EntityNames.Module, MyModule.ModuleId))
            {
                MyModule = await _MyModuleService.UpdateMyModuleAsync(MyModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MyModule Put Attempt {MyModule}", MyModule);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                MyModule = null;
            }
            return MyModule;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            Models.MyModule MyModule = await _MyModuleService.GetMyModuleAsync(id, moduleid);
            if (MyModule != null && IsAuthorizedEntityId(EntityNames.Module, MyModule.ModuleId))
            {
                await _MyModuleService.DeleteMyModuleAsync(id, MyModule.ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized v Delete Attempt {MyModuleId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
