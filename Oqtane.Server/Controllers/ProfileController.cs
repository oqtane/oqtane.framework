using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IProfileRepository Profiles;

        public ProfileController(IProfileRepository Profiles)
        {
            this.Profiles = Profiles;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Profile> Get(string siteid)
        {
            if (siteid == "")
            {
                return Profiles.GetProfiles();
            }
            else
            {
                return Profiles.GetProfiles(int.Parse(siteid));
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Profile Get(int id)
        {
            return Profiles.GetProfile(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Profile Post([FromBody] Profile Profile)
        {
            if (ModelState.IsValid)
            {
                Profile = Profiles.AddProfile(Profile);
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
                Profile = Profiles.UpdateProfile(Profile);
            }
            return Profile;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            Profiles.DeleteProfile(id);
        }
    }
}
