using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Oqtane.Security
{
    public class DataProtectorTests
    {
        [Theory]
        [InlineData("Oqtane")]
        [InlineData("Oqtane Framework")]
        public void ProtectData(string data)
        {
            // Arrange
            var serviceProvider = GetServiceProvider();
            var dataProtector = serviceProvider.GetService<DataProtector>();

            // Act
            var protectedData = dataProtector.Protect(data);

            //Assert
            Assert.NotEmpty(protectedData);
        }

        [Theory]
        [InlineData("CfDJ8JMKOwNQ58dGvkWTQhUZUsMZG0CtwOWYebvGhEX59zmRKLvcFfqNaxHUEqhzIWBKv851iYokPt5E4UtN9orICXyhTKjuzm8ioqJQPr5lYhJ_yYF2AUpsNdsGcbmSxVNPQg", "Oqtane")]
        [InlineData("CfDJ8JMKOwNQ58dGvkWTQhUZUsMIFpxoXyJyuNperxexjGrCvLHiWCNhsaHuugzyY2E4uhU1XvnppoUdoUj5PN-M5R10RmMt3dIAbuwzt2a4NChVasq6uZzL9mTPFTm_As81DlJzQWZ0UXzGYiAAIGy1QUY", "Oqtane Framework")]
        public void UnprotectData(string protectedData, string expectedData)
        {
            // Arrange
            var serviceProvider = GetServiceProvider();
            var dataProtector = serviceProvider.GetService<DataProtector>();

            // Act
            var data = dataProtector.Unprotect(protectedData);

            //Assert
            Assert.Equal(expectedData, data);
        }

        private static ServiceProvider GetServiceProvider()
            => new ServiceCollection()
                .AddDataProtection()
                .Services
                .AddSingleton<DataProtector>()
                .BuildServiceProvider();
    }
}
