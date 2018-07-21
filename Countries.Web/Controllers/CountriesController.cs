using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Countries.Web.Contracts;
using System.Collections.Generic;

namespace Countries.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountriesService _countriesService;

        public CountriesController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        [HttpGet]
        public async Task<ActionResult<List<string>>> GetAll()
        {
            var result = await _countriesService.GetAsync().ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("{countryName}")]
        public async Task<object> GetCountry(string countryName)
        {
            var result = await _countriesService.GetAsync(countryName).ConfigureAwait(false);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}