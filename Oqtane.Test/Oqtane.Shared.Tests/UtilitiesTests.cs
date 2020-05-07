using Oqtane.Shared;
using Xunit;

namespace Oqtane.Test.Oqtane.Shared.Tests
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("contoso", "login", "returnUrl=/admin", "/contoso/login?returnUrl=/admin")]
        [InlineData("contoso", "admin", "", "/contoso/admin")]
        [InlineData("contoso", "", "pageId=4", "/contoso?pageId=4")]
        [InlineData("contoso", "", "pageId=4&moduleId=10", "/contoso?pageId=4&moduleId=10")]
        [InlineData("contoso", "", "", "/contoso")]
        [InlineData("", "login", "returnUrl=/admin", "/login?returnUrl=/admin")]
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
    }
}
