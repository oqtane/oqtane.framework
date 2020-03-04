using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IProfileRepository _profiles;
        private readonly ILogManager _logger;

        public ProfileController(IProfileRepository Profiles, ILogManager logger)
        {
            this._profiles = Profiles;
            this._logger = logger;
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
        public Profile Post([FromBody] Profile Profile)
        {
            if (ModelState.IsValid)
            {
                Profile = _profiles.AddProfile(Profile);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Profile Added {Profile}", Profile);
            }
            return Profile;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Profile Put(int id, [FromBody] Profile Profile)
        {
            if (ModelState.IsValid)
            {
                Profile = _profiles.UpdateProfile(Profile);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Profile Updated {Profile}", Profile);
            }
            return Profile;
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
