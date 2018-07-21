using Countries.Web.Contracts;
using Countries.Web.Models;
using Countries.Web.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Countries.Tests.ServiceTests
{
    public class RestCountriesServiceTests
    {
        private IRestCountriesService Sut { get; set; }

        public RestCountriesServiceTests()
        {
            var httpClient = new HttpClient();

            var loggerMock = new Mock<ILogger<RestCountriesService>>();

            Sut = new RestCountriesService(httpClient, loggerMock.Object);
        }

        [Fact]
        public async Task GetTest()
        {
            var result = await Sut.GetAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.Any());
        }

        [Fact]
        public async Task GetCountryTest()
        {
            var countryName = "Finland";

            var result = await Sut.GetAsync(countryName).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal(countryName, result.Name);
        }

        [Fact]
        public async Task GetUnknownCountryTest()
        {
            var countryName = "FooBar";
            await Assert.ThrowsAsync<HttpRequestException>(() => Sut.GetAsync(countryName)).ConfigureAwait(false);
        }
    }
}
