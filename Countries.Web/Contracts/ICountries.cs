using Countries.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Countries.Web.Contracts
{
    public interface ICountries
    {
        Task<List<string>> GetAsync();
        Task<Country> GetAsync(string countryName);
    }
}
