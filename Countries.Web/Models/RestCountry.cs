using Countries.Web.Models;

namespace Countries.Web.Models
{
    public class RestCountry : CountryBase
    {
        public string[] topLevelDomain { get; set; }
        public string alpha2Code { get; set; }
        public string alpha3Code { get; set; }
        public string[] callingCodes { get; set; }
        public string capital { get; set; }
        public string[] altSpellings { get; set; }
        public string region { get; set; }
        public string subregion { get; set; }
        public int population { get; set; }
        public float[] latlng { get; set; }
        public string demonym { get; set; }
        public float area { get; set; }
        public float gini { get; set; }
        public string[] timezones { get; set; }
        public string[] borders { get; set; }
        public string nativeName { get; set; }
        public string numericCode { get; set; }
        public Currency[] currencies { get; set; }
        public Language[] languages { get; set; }
        public Translations translations { get; set; }
        public string flag { get; set; }
        public Regionalbloc[] regionalBlocs { get; set; }
        public string cioc { get; set; }

        public class Translations
        {
            public string de { get; set; }
            public string es { get; set; }
            public string fr { get; set; }
            public string ja { get; set; }
            public string it { get; set; }
            public string br { get; set; }
            public string pt { get; set; }
            public string nl { get; set; }
            public string hr { get; set; }
            public string fa { get; set; }
        }

        public class Currency
        {
            public string code { get; set; }
            public string name { get; set; }
            public string symbol { get; set; }
        }

        public class Language
        {
            public string iso639_1 { get; set; }
            public string iso639_2 { get; set; }
            public string name { get; set; }
            public string nativeName { get; set; }
        }

        public class Regionalbloc
        {
            public string acronym { get; set; }
            public string name { get; set; }
            public object[] otherAcronyms { get; set; }
            public object[] otherNames { get; set; }
        }
    }
}
