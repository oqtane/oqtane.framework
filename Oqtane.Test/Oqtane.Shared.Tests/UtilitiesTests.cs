using Oqtane.Shared;
using Xunit;

namespace Oqtane.Test.Oqtane.Shared.Tests
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("contoso", "login", "returnUrl=/admin", "http://contoso/login?returnUrl=/admin")]
        [InlineData("contoso", "admin", "", "http://contoso/admin")]
        [InlineData("contoso", "", "pageId=4", "http://contoso/?pageId=4")]
        [InlineData("contoso", "", "", "http://contoso/")]
        [InlineData("http://contoso", "login", "returnUrl=/admin", "http://contoso/login?returnUrl=/admin")]
        [InlineData("http://contoso", "admin", "", "http://contoso/admin")]
        [InlineData("http://contoso", "", "pageId=4", "http://contoso/?pageId=4")]
        [InlineData("http://contoso", "", "", "http://contoso/")]
        [InlineData("https://contoso", "login", "returnUrl=/admin", "https://contoso/login?returnUrl=/admin")]
        [InlineData("https://contoso", "admin", "", "https://contoso/admin")]
        [InlineData("https://contoso", "", "pageId=4", "https://contoso/?pageId=4")]
        [InlineData("https://contoso", "", "", "https://contoso/")]
        [InlineData("", "login", "returnUrl=/admin", "http://localhost/login?returnUrl=/admin")]
        [InlineData("", "admin", "", "http://localhost/admin")]
        [InlineData("", "", "pageId=4", "http://localhost/?pageId=4")]
        [InlineData("", "", "", "http://localhost/")]
        public void NavigateUrlTest(string alias, string path, string parameters, string expectedUrl)
        {
            // Arrange
            var navigatedUrl = string.Empty;

            // Act
            navigatedUrl = Utilities.NavigateUrl(alias, path, parameters);

            // Assert
            Assert.Equal(expectedUrl, navigatedUrl);
        }
    }
}
