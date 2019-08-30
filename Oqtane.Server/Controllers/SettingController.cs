using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SettingController : Controller
    {
        private readonly ISettingRepository Settings;

        public SettingController(ISettingRepository Settings)
        {
            this.Settings = Settings;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Setting> Get(string entityname, int entityid)
        {
            return Settings.GetSettings(entityname, entityid);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Setting Get(int id)
        {
            return Settings.GetSetting(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Setting Post([FromBody] Setting Setting)
        {
            if (ModelState.IsValid)
            {
                Setting = Settings.AddSetting(Setting);
            }
            return Setting;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Setting Put(int id, [FromBody] Setting Setting)
        {
            if (ModelState.IsValid)
            {
                Setting = Settings.UpdateSetting(Setting);
            }
            return Setting;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            Settings.DeleteSetting(id);
        }
    }
}
