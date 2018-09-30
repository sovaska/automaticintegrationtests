using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Countries.Web.Contracts;
using Countries.Web.Models;

namespace Countries.Web.Repositories
{
    public class CountriesInMemoryRepository : ICountriesInMemoryRepository
    {
        private readonly ConcurrentDictionary<string, string> _countryNames = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, Country> _countries = new ConcurrentDictionary<string, Country>();

        public Task<List<string>> GetAsync()
        {
            var countries = _countryNames.Keys.ToList();
            countries.Sort();
            return Task.FromResult(countries);
        }

        public Task<Country> GetAsync(string countryName)
        {
            if (!_countries.ContainsKey(countryName))
            {
                Country result = null;
                return Task.FromResult(result);
            }

            return Task.FromResult(_countries[countryName]);
        }

        public void Set(List<string> countries)
        {
            _countryNames.Clear();

            foreach (var countryName in countries)
            {
                _countryNames.TryAdd(countryName, countryName);
            }
        }

        public void Set(string countryName, Country country)
        {
            if (_countries.ContainsKey(countryName))
            {
                _countries.TryRemove(countryName, out var currentCountry);
            }
            _countries.TryAdd(countryName, country);
        }
    }
}
