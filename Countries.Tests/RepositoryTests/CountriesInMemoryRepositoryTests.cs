using Countries.Web.Contracts;
using Countries.Web.Models;
using Countries.Web.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Countries.Tests.RepositoryTests
{
    public class CountriesInMemoryRepositoryTests
    {
        private ICountriesInMemoryRepository Sut { get; set; }

        public CountriesInMemoryRepositoryTests()
        {
            Sut = new CountriesInMemoryRepository();
        }

        [Fact]
        public async Task EmptyCountriesTest()
        {
            var result = await Sut.GetAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task OneCountryTest()
        {
            var countryName = "Finland";

            Sut.Set(new List<string> { countryName });

            var result = await Sut.GetAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(countryName, result.FirstOrDefault());
        }

        [Fact]
        public async Task ListOfCountriesTest()
        {
            var countryName = "Finland";

            Sut.Set(new List<string> { countryName });
            Sut.Set(new List<string> { countryName });

            var result = await Sut.GetAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(countryName, result.FirstOrDefault());
        }

        [Fact]
        public async Task ClearCountryListTest()
        {
            Sut.Set(new List<string>());

            var result = await Sut.GetAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task ReplaceCountryDataTest()
        {
            var countryName = "Finland";

            Sut.Set(new List<string> { countryName });
            Sut.Set(new List<string> { countryName });

            var result = await Sut.GetAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(countryName, result.FirstOrDefault());
        }

        [Fact]
        public async Task CountryDoesNotExistTest()
        {
            var countryName = "Finland";

            var result = await Sut.GetAsync(countryName).ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task CountryExistsTest()
        {
            var countryName = "Finland";

            Sut.Set(countryName, new Country());

            var result = await Sut.GetAsync(countryName).ConfigureAwait(false);

            Assert.NotNull(result);
        }
    }
}
