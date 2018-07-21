using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using Moq;
using System.Collections.Generic;
using Countries.Web.Models;

namespace Countries.Tests.ControllerTests
{
    public class BasicTests : IClassFixture<CustomWebApplicationFactory<Web.Startup>>
    {
        private readonly CustomWebApplicationFactory<Web.Startup> _factory;

        private HttpClient Client { get; set; }

        public BasicTests(CustomWebApplicationFactory<Web.Startup> factory)
        {
            _factory = factory;
            Client = _factory.CreateClient();
        }

        [Theory]
        [InlineData("/api/countries/Finland")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            _factory.CountriesInMemoryRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(new List<string> { "Finland" });
            _factory.CountriesServiceMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(new Country());

            // Act
            var response = await Client.GetAsync(url).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/api/sites/S-03144")]
        public async Task SiteNotFoundTest(string url)
        {
            // Arrange
            _factory.CountriesInMemoryRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(new List<string>());
            Country ret = null;
            _factory.CountriesServiceMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(ret);

            // Act
            var response = await Client.GetAsync(url).ConfigureAwait(false);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
