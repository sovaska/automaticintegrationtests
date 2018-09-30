using Xunit;
using System.Threading.Tasks;
using System.Net;
using Moq;
using System.Collections.Generic;
using Countries.Web.Models;

namespace Countries.Tests.ControllerTests
{
    public class AuthenticationTests : ControllerTestBase, IClassFixture<CustomWebApplicationFactory<Web.Startup>>
    {
        private readonly CustomWebApplicationFactory<Web.Startup> _factory;

        public AuthenticationTests(CustomWebApplicationFactory<Web.Startup> factory)
            : base(factory.CreateClient())
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("", HttpStatusCode.Unauthorized)]
        public async Task Test(string authScheme, HttpStatusCode expectedStatusCode)
        {
            _factory.CountriesServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(new List<string>());
            _factory.CountriesServiceMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(new Country { Name = "Finland" });

            var metadata = await ReadMetadataAsync().ConfigureAwait(false);

            foreach (var action in metadata.Actions)
            {
                await TestSingleActionAsync(authScheme, expectedStatusCode, action).ConfigureAwait(false);
            }
        }
    }
}
