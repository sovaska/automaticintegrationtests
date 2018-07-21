using Countries.Web.Models;

namespace Countries.Web.Models
{
    public class Country : CountryBase
    {
        public string Capital { get; internal set; }
    }
}
