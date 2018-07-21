using Moq;
using System.Threading.Tasks;
using Countries.Web.Contracts;
using Countries.Web.Services;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Countries.Web.Models;

namespace Countries.Tests.ServiceTests
{
    public class CountriesServiceTests
    {
        private ICountriesService Sut { get; set; }
        private Mock<IRestCountriesService> RestCountiesServiceMock { get; } = new Mock<IRestCountriesService>();
        private Mock<ICountriesInMemoryRepository> CountriesInMemoryRepositoryMock { get; } = new Mock<ICountriesInMemoryRepository>();
        
        public CountriesServiceTests()
        {
            Sut = new CountriesService(RestCountiesServiceMock.Object, CountriesInMemoryRepositoryMock.Object);
        }

        [Fact]
        public async Task NoCountriesTest()
        {
            CountriesInMemoryRepositoryMock.Setup(e => e.GetAsync()).ReturnsAsync(new List<string>());
            RestCountiesServiceMock.Setup(e => e.GetAsync()).ReturnsAsync(new List<string>());

            var result = await Sut.GetAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task SingleCountryTest()
        {
            var countryName = "Finland";
            CountriesInMemoryRepositoryMock.Setup(e => e.GetAsync()).ReturnsAsync(new List<string>());
            RestCountiesServiceMock.Setup(e => e.GetAsync()).ReturnsAsync(new List<string> { countryName });

            var result = await Sut.GetAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.Equal(countryName, result.FirstOrDefault());
        }

        [Fact]
        public async Task CountryNameNullTest()
        {
            Country ret = null;
            RestCountiesServiceMock.Setup(e => e.GetAsync(It.IsAny<string>())).ReturnsAsync(ret);

            var result = await Sut.GetAsync(null).ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task CountryNameEmptyTest()
        {
            Country ret = null;
            RestCountiesServiceMock.Setup(e => e.GetAsync(It.IsAny<string>())).ReturnsAsync(ret);

            var result = await Sut.GetAsync(string.Empty).ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task CountryNameNotEmptyTest()
        {
            RestCountiesServiceMock.Setup(e => e.GetAsync(It.IsAny<string>())).ReturnsAsync(new Country());

            var result = await Sut.GetAsync(string.Empty).ConfigureAwait(false);

            Assert.NotNull(result);
        }
    }
}
