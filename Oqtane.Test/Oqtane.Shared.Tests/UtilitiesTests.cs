using System;
using System.Globalization;
using Oqtane.Shared;
using Xunit;

namespace Oqtane.Test.Oqtane.Shared.Tests
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("contoso", "login", "returnUrl=/admin", "/contoso/login/!/returnUrl=/admin")]
        [InlineData("contoso", "admin", "", "/contoso/admin")]
        [InlineData("contoso", "", "pageId=4", "/contoso?pageId=4")]
        [InlineData("contoso", "", "pageId=4&moduleId=10", "/contoso?pageId=4&moduleId=10")]
        [InlineData("contoso", "", "", "/contoso")]
        [InlineData("", "login", "returnUrl=/admin", "/login/!/returnUrl=/admin")]
        [InlineData("", "admin", "", "/admin")]
        [InlineData("", "", "pageId=4", "/?pageId=4")]
        [InlineData("", "", "pageId=4&moduleId=10", "/?pageId=4&moduleId=10")]
        [InlineData("", "", "", "/")]
        public void NavigateUrlTest(string alias, string path, string parameters, string expectedUrl)
        {
            // Arrange
            var navigatedUrl = string.Empty;

            // Act
            navigatedUrl = Utilities.NavigateUrl(alias, path, parameters);

            // Assert
            Assert.Equal(expectedUrl, navigatedUrl);
        }

        [Theory]
        [InlineData(2022, 02, 01, "21:00", "Eastern Standard Time", 2022, 2, 2, 2)]
        [InlineData(2022, 02, 02, "15:00", "Eastern Standard Time", 2022, 2, 2, 20)]
        [InlineData(2022, 02, 02, "", "Eastern Standard Time", 2022, 2, 2, 5)]
        [InlineData(0, 0, 0, "", "Eastern Standard Time", 0, 0, 0, 0)]
        public void LocalDateAndTimeAsUtcTest(int yr, int mo, int day, string timeString, string zone, int yrUtc, int moUtc, int dayUtc, int hrUtc)
        {
            // Arrange
            DateTime? srcDate = null;
            if (yr > 0)
            {
                srcDate = new DateTime(yr, mo, day);
            }

            // Act
            var dateTime = Utilities.LocalDateAndTimeAsUtc(srcDate, timeString, TimeZoneInfo.FindSystemTimeZoneById(zone));

            // Assert
            DateTime? expected = null;
            if (yrUtc > 0)
            {
                expected = new DateTime(yrUtc, moUtc, dayUtc, hrUtc, 0, 0, DateTimeKind.Utc);
            }
            Assert.Equal(expected, dateTime);
        }

        [Theory]
        // Standard Time
        [InlineData(2022, 2, 2, 2, DateTimeKind.Unspecified, "Eastern Standard Time", "2022/02/01", "21:00")]
        [InlineData(2022, 2, 2, 2, DateTimeKind.Utc, "Eastern Standard Time", "2022/02/01", "21:00")]
        [InlineData(2022, 2, 2, 20, DateTimeKind.Unspecified, "Eastern Standard Time", "2022/02/02", "15:00")]
        [InlineData(2022, 2, 2, 20, DateTimeKind.Utc, "Eastern Standard Time", "2022/02/02", "15:00")]
        [InlineData(2022, 2, 2, 5, DateTimeKind.Unspecified, "Eastern Standard Time", "2022/02/02", "")]
        [InlineData(2022, 2, 2, 5, DateTimeKind.Utc, "Eastern Standard Time", "2022/02/02", "")]
        // Daylight Savings Time
        [InlineData(2022, 7, 2, 20, DateTimeKind.Unspecified, "Eastern Standard Time", "2022/07/02", "16:00")]
        [InlineData(2022, 7, 2, 20, DateTimeKind.Utc, "Eastern Standard Time", "2022/07/02", "16:00")]
        [InlineData(2022, 7, 2, 4, DateTimeKind.Unspecified, "Eastern Standard Time", "2022/07/02", "")]
        [InlineData(2022, 7, 2, 4, DateTimeKind.Utc, "Eastern Standard Time", "2022/07/02", "")]
        public void UtcAsLocalDateAndTimeTest(int yr, int mo, int day, int hr, DateTimeKind dateTimeKind, string zone, string expectedDate, string expectedTime)
        {
            // Arrange
            DateTime? srcDate = null;
            if (yr > 0)
            {
                srcDate = new DateTime(yr, mo, day, hr, 0, 0, dateTimeKind);
            }

            // Act
            var dateAndTime = Utilities.UtcAsLocalDateAndTime(srcDate, TimeZoneInfo.FindSystemTimeZoneById(zone));

            // Assert
            Assert.Equal(expectedDate, dateAndTime.date.Value.ToString("yyyy/MM/dd"));
            Assert.Equal(expectedTime, dateAndTime.time);
        }
    }
}
