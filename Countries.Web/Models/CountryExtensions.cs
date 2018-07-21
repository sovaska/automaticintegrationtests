namespace Countries.Web.Models
{
    public static class CountryExtensions
    {
        public static Country ToCountry(this RestCountry restCountry)
        {
            return new Country
            {
                Name = restCountry.Name,
                Capital = restCountry.capital
            };
        }
    }
}
