using Xunit;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Countries.Tests.ControllerTests
{
    public class DependencyInjectionTests : ControllerTestBase, IClassFixture<WebApplicationFactory<Web.Startup>>
    {
        public DependencyInjectionTests(WebApplicationFactory<Web.Startup> factory)
            : base(factory.CreateClient())
        {
        }

        [Fact]
        public async Task Test()
        {
            var diMetadata = await ReadDependencyInjectionMetadataAsync().ConfigureAwait(false);

            Assert.NotNull(diMetadata);
            Assert.True(!diMetadata.Problems.Any());
        }
    }
}
