using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Countries.Web.Contracts;

namespace Countries.Tests.ControllerTests
{
    public class CustomWebApplicationFactory<TStartup>
       : WebApplicationFactory<Web.Startup>
    {
        public Mock<ICountriesInMemoryRepository> CountriesInMemoryRepositoryMock { get; private set; }
        public Mock<ICountriesService> CountriesServiceMock { get; private set; }

        public CustomWebApplicationFactory()
        {
            CountriesInMemoryRepositoryMock = new Mock<ICountriesInMemoryRepository>();
            CountriesServiceMock = new Mock<ICountriesService>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<ICountriesInMemoryRepository>(CountriesInMemoryRepositoryMock.Object);
                services.AddSingleton<ICountriesService>(CountriesServiceMock.Object);
            });
        }
    }
}
