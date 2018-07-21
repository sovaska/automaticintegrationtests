using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Countries.Web.Contracts;
using Countries.Web.Models;

namespace Countries.Web.Services
{
    public class RestCountriesService : IRestCountriesService
    {
        private HttpClient _client;
        private ILogger<RestCountriesService> _logger;

        public RestCountriesService(HttpClient client, ILogger<RestCountriesService> logger)
        {
            _client = client;
            _client.BaseAddress = new Uri($"https://restcountries.eu/rest/v2/");
            _logger = logger;
        }

        public async Task<List<string>> GetAsync()
        {
            try
            {
                var url = new Uri($"all", UriKind.Relative);
                _logger.LogWarning($"HttpClient: Loading {url}");
                using (var res = await _client.GetAsync(url).ConfigureAwait(false))
                {
                    using (res.EnsureSuccessStatusCode())
                    {
                        var countries = await res.Content.ReadAsAsync<List<CountryBase>>().ConfigureAwait(false);

                        return countries.Select(country => country.Name).ToList();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to RestCountries API {ex.ToString()}");
                throw;
            }
        }

        public async Task<Country> GetAsync(string countryName)
        {
            try
            {
                var url = new Uri($"name/{countryName}", UriKind.Relative);
                _logger.LogWarning($"HttpClient: Loading {url}");
                using (var res = await _client.GetAsync(url).ConfigureAwait(false))
                {
                    using (res.EnsureSuccessStatusCode())
                    {
                        var country = (await res.Content.ReadAsAsync<List<RestCountry>>().ConfigureAwait(false)).FirstOrDefault();
                        if (country == null)
                        {
                            return null;
                        }
                        return country.ToCountry();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to RestCountries API {ex.ToString()}");
                throw;
            }
        }
    }
}
