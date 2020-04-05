using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IProfileRepository _profiles;
        private readonly ILogManager _logger;

        public ProfileController(IProfileRepository profiles, ILogManager logger)
        {
            _profiles = profiles;
            _logger = logger;
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
        [Authorize(Roles = Constants.AdminRole)]
        public Profile Post([FromBody] Profile profile)
        {
            if (ModelState.IsValid)
            {
                profile = _profiles.AddProfile(profile);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Profile Added {Profile}", profile);
            }
            return profile;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Profile Put(int id, [FromBody] Profile profile)
        {
            if (ModelState.IsValid)
            {
                profile = _profiles.UpdateProfile(profile);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Profile Updated {Profile}", profile);
            }
            return profile;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            _profiles.DeleteProfile(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Profile Deleted {ProfileId}", id);
        }
    }
}
