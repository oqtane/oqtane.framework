using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Xunit;

namespace Oqtane.Oqtane.Client.Tests
{
    public class LocalizationCookieTests
    {
        [Theory]
        [InlineData("c=ar|uic=ar", "ar")]
        [InlineData("c=ar", null)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void ParseCookie(string localizationCookie, string expectedCulture)
        {
            // Arrange
            var localizationCookieValue = CookieRequestCultureProvider.ParseCookieValue(localizationCookie);

            // Act
            var culture = localizationCookieValue?.UICultures?[0].Value;

            // Assert
            Assert.Equal(expectedCulture, culture);
        }
    }
}
