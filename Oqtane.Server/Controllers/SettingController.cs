using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

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
        public void Delete(int id)
        {
            Settings.DeleteSetting(id);
        }
    }
}
