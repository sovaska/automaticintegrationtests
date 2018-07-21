using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Countries.Web.Contracts;
using Countries.Web.Models;

namespace Countries.Web.Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ICountriesInMemoryRepository _repository;
        private readonly IRestCountriesService _restCountriesService;

        public CountriesService(IRestCountriesService restCountriesService, ICountriesInMemoryRepository repository)
        {
            _repository = repository;
            _restCountriesService = restCountriesService;
        }

        public async Task<List<string>> GetAsync()
        {
            // Try to read list of countries from cache
            var countries = await _repository.GetAsync().ConfigureAwait(false);

            if (!countries.Any())
            {
                // Read countries from REST endpoint
                countries = await _restCountriesService.GetAsync().ConfigureAwait(false);

                // Save countries to cache
                _repository.Set(countries);
            }

            return countries;
        }

        public async Task<Country> GetAsync(string countryName)
        {
            // Try to read country from cache
            var country = await _repository.GetAsync(countryName).ConfigureAwait(false);

            if (country == null)
            {
                // Read country from REST endpoint
                country = await _restCountriesService.GetAsync(countryName).ConfigureAwait(false);

                // Save country to cache
                _repository.Set(countryName, country);
            }

            return country;
        }
    }
}