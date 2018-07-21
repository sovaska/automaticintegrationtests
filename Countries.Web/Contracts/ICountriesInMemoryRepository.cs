using Countries.Web.Models;
using System.Collections.Generic;

namespace Countries.Web.Contracts
{
    public interface ICountriesInMemoryRepository : ICountries
    {
        void Set(List<string> countries);
        void Set(string countryName, Country country);
    }
}
