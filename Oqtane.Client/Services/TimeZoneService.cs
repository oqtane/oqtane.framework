using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using NodaTime.TimeZones;
using NodaTime;
using Oqtane.Documentation;
using NodaTime.Extensions;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve <see cref="TimeZone"/> entries
    /// </summary>
    public interface ITimeZoneService
    {
        /// <summary>
        /// Get the list of time zones
        /// </summary>
        /// <returns></returns>
        List<Models.TimeZone> GetTimeZones();
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class TimeZoneService : ITimeZoneService
    {
        private readonly IStringLocalizer<TimeZoneResources> _TimeZoneLocalizer;

        public TimeZoneService(IStringLocalizer<TimeZoneResources> TimeZoneLocalizer)
        {
            _TimeZoneLocalizer = TimeZoneLocalizer;
        }

        public List<Models.TimeZone> GetTimeZones()
        {
            var timezones = new List<Models.TimeZone>();

            foreach (var tz in DateTimeZoneProviders.Tzdb.GetAllZones()
                // only include timezones which have a country code defined or are US timezones
                .Where(item => !string.IsNullOrEmpty(TzdbDateTimeZoneSource.Default.ZoneLocations.FirstOrDefault(l => l.ZoneId == item.Id)?.CountryCode) || item.Id.ToLower().Contains("us/"))
                // order by UTC offset (ie. -11:00 to +14:00)
                .OrderBy(item => item.GetUtcOffset(Instant.FromDateTimeUtc(DateTime.UtcNow)).Ticks))
            {
                // get localized display name
                var displayname = _TimeZoneLocalizer[tz.Id].Value;
                if (displayname == tz.Id)
                {
                    // use default "friendly" display format
                    displayname =  displayname.Replace("_", " ").Replace("/", " / ");
                }

                // time zones can be excluded from the list by providing an empty translation in the localization file
                if (!string.IsNullOrEmpty(displayname))
                {
                    // include offset prefix
                    var offset = tz.GetUtcOffset(Instant.FromDateTimeUtc(DateTime.UtcNow)).Ticks;
                    displayname = "(UTC" + (offset >= 0 ? "+" : "-") + new DateTime(Math.Abs(offset)).ToString("HH:mm") + ") " + displayname;

                    timezones.Add(new Models.TimeZone()
                    {
                        Id = tz.Id,
                        DisplayName = displayname
                    });
                }
            }

            return timezones;
        }
    }
}
