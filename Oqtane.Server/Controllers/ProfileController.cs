using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class ProfileController : Controller
    {
        private readonly IProfileRepository _profiles;
        private readonly ILogManager _logger;
        private readonly IStringLocalizer _localizer;

        public ProfileController(IProfileRepository profiles, ILogManager logger, IStringLocalizer<ProfileController> localizer)
        {
            _profiles = profiles;
            _logger = logger;
            _localizer = localizer;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Profile> Get(string siteid)
        {
            return _profiles.GetProfiles(int.Parse(siteid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Profile Get(int id)
        {
            return _profiles.GetProfile(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public Profile Post([FromBody] Profile profile)
        {
            if (ModelState.IsValid)
            {
                profile = _profiles.AddProfile(profile);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, _localizer["Profile Added {Profile}"], profile);
            }
            return profile;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public Profile Put(int id, [FromBody] Profile profile)
        {
            if (ModelState.IsValid)
            {
                profile = _profiles.UpdateProfile(profile);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, _localizer["Profile Updated {Profile}"], profile);
            }
            return profile;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Delete(int id)
        {
            _profiles.DeleteProfile(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, _localizer["Profile Deleted {ProfileId}"], id);
        }
    }
}
