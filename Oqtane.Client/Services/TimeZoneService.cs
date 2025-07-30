using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class TimeZoneService : ITimeZoneService
    {
        private readonly IStringLocalizer<TimeZoneResources> _TimeZoneLocalizer;

        public TimeZoneService(IStringLocalizer<TimeZoneResources> TimeZoneLocalizer)
        {
            _TimeZoneLocalizer = TimeZoneLocalizer;
        }

        public List<TimeZone> GetTimeZones()
        {
            var _timezones = new List<TimeZone>();
            foreach (var timezone in Utilities.GetTimeZones())
            {
                _timezones.Add(new TimeZone
                {
                    Id = timezone.Id,
                    DisplayName = _TimeZoneLocalizer[timezone.Id]
                });
            }
            return _timezones.OrderBy(item => item.DisplayName).ToList();
        }
    }
}
