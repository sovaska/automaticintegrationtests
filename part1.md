# What

This post is divided into couple of separate articles. Target is to study some new features introduced in AspDotNet 2.1:

- IHttpClientFactory
- Polly (not part of AspDotNet but is related)
- WebApplicationFactory for integration tests

## Goal

The goal is to build integation test that validates Authentication is enabled in all API Controller actions. This is to make sure some development time code is not published into production by mistake.

Web application also has UI, but it doesn't do anything at the moment. 

There is only two Controllers in Web application. First one will return metadata information about Web application and another one returns information about countries.

## Building REST endpoint for getting metadata about controller actions

First step is to build REST endpoint for getting information about all controllers and all controller actions. Endpoint is called /api/metadata:

    using Microsoft.AspNetCore.Mvc;
    using Countries.Web.Contracts;
    
    namespace Countries.Web.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class MetadataController : ControllerBase
        {
            private readonly IMetadataService _metadataService;

            public MetadataController(IMetadataService metadataService)
            {
                _metadataService = metadataService;
            }

            [HttpGet]
            public ActionResult<Models.Metadata> GetActions()
            {
                var result = _metadataService.GetMetadata();
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
        }
    }

The actual implementation for getting controllers and actions can be found in MetadataService class:

    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Countries.Web.Contracts;
    using Countries.Web.Models;

    namespace Countries.Web.Services
    {
        public class MetadataService : IMetadataService
        {
            private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

            public MetadataService(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
            {
                _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            }

            public Metadata GetMetadata()
            {
                var result = new Metadata();

                var items = _actionDescriptorCollectionProvider.ActionDescriptors.Items;
                for (var i = 0; i < items.Count; i++)
                {
                    var actionDescriptor = items[i];
                    if (!(actionDescriptor is ControllerActionDescriptor))
                    {
                        continue;
                    }

                    var action = ParseAction(actionDescriptor);
                    result.Actions.Add(action);
                }

                return result;
            }

            private static Action ParseAction(Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor)
            {
                var action = new Action
                {
                    Template = actionDescriptor.AttributeRouteInfo.Template,
                    Parameters = actionDescriptor.Parameters
                        .Select(p => new Parameter { Name = p.Name, Type = p.ParameterType.FullName })
                        .ToList()
                };

                if (actionDescriptor.ActionConstraints != null)
                {
                    for (var i = 0; i < actionDescriptor.ActionConstraints.Count; i++)
                    {
                        var actionDescriptorActionConstraint = actionDescriptor.ActionConstraints[i];
                        if (actionDescriptorActionConstraint is HttpMethodActionConstraint httpMethodActionConstraint)
                        {
                            action.Methods.AddRange(httpMethodActionConstraint.HttpMethods);
                        }
                    }
                }

                return action;
            }
        }
    }
    
You need to remember register service in Startup class ConfigureServices -method:

    services.AddSingleton<IMetadataService, MetadataService>();

For this Web application endpoint returns following metadata as JSON:

    {
        "actions": [
            {
                "template": "api/Countries",
                "methods": [
                    "GET"
                ],
                "parameters": []
            },{
                "template": "api/Countries/{countryName}",
                "methods": [
                    "GET"
                ],
                "parameters": [
                    {
                        "name": "countryName",
                        "type": "System.String"
                    }
                ]
            },{
                "template": "api/Metadata",
                "methods": [
                    "GET"
                ],
                "parameters": []
            }
        ]
    }
    
We will use this metadata in integration test to build Http Requests and then call each request, and validate endpoint returns 401. If not, one of the REST endpoints does not have authorization enabled.

## Building integration test that uses metadata endpoint

I'm using [XUnit](https://xunit.github.io/) as my test framework, and [Moq](https://github.com/Moq/moq4) to mock some interfaces during testing.

The following steps are needed

1. Create Test specific Startup class (derived from WebApplicationFactory) to mock some services during test execution
2. Create base class for Controller tests. Base class contains methods for getting metadata, building urls, executing requests, validating headers (not used at the moment) and validating cookies (not used at the moment)
3. Create test class that initializes mocked services, and then uses base class to exexute requests

### Test specific startup class

Override ConfigureWebHost -method, and especially use IWebHostBuilder.ConfigureTestServices. Here it is possible to override services that are Configured in actual startup class. I'm using mocked versions of two interfaces.

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

### Base class for Controller tests

Class contains methods for getting metadata from metadata controller, using metadata to build urls and finally executing requests by using urls.

Note: this implementation does not validate response headers or cookies. Also it does not support authentication at the moment. These are somehing I'm planning to add later on.

    using Xunit;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System;
    using System.Net.Http.Headers;
    using System.Collections.Generic;
    using System.Linq;
    using Countries.Web.Models;
    using Newtonsoft.Json;
    using System.Net;
    
    namespace Countries.Tests.ControllerTests
    {
        public abstract class ControllerTestBase
        {
            protected HttpClient Client { get; private set; }
    
            protected ControllerTestBase(HttpClient httpClient)
            {
                Client = httpClient;
            }
    
            protected void ValidateHeaders(HttpHeaders responseHeaders)
            {
                return;
                // Note: Currently not validating headers
                //Assert.True(responseHeaders.TryGetValues("X-Frame-Options", out var headers));
    
                //var enumerable = headers as IList<string> ?? headers.ToList();
                //Assert.Equal(1, enumerable.Count);
                //Assert.Equal("SAMEORIGIN", enumerable.FirstOrDefault());
    
                //Assert.True(responseHeaders.TryGetValues("Strict-Transport-Security", out headers));
                //Assert.Single(headers);
                //Assert.Equal("max-age=31536000; includeSubDomains; preload", headers.FirstOrDefault());
            }
    
            protected void ValidateCookie(string cookie)
            {
                var split = cookie.Split(';');
                Assert.Equal(3, split.Length);
    
                foreach (var s in split)
                {
                    var val = s.Trim();
                    if (val.StartsWith("path=/"))
                    {
                        // Do nothing
                    }
                    else if (val.Equals("secure"))
                    {
                        // Do nothing
                    }
                    else if (val.Equals("httponly"))
                    {
                        // Do nothing
                    }
                    else
                    {
                        Assert.True(false, "Unknown cookie parameter");
                    }
                }
            }
    
            protected async Task<Metadata> ReadMetadataAsync()
            {
                var metadata = await ReadActionMetadataAsync().ConfigureAwait(false);
                return metadata;
            }
    
            protected static Uri BuildUri(string controller, string baseUri = "http://localhost/api/", string parameters = "")
            {
                return new Uri($"{baseUri}{controller}{parameters}");
            }
    
            private async Task<Metadata> ReadActionMetadataAsync()
            {
                using (var msg = new HttpRequestMessage(HttpMethod.Get, BuildUri("Metadata").ToString()))
                {
                    using (var response = await Client.SendAsync(msg).ConfigureAwait(false))
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
                        if (!response.IsSuccessStatusCode)
                        {
                            return null;
                        }
    
                        ValidateHeaders(response.Headers);
    
                        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (string.IsNullOrEmpty(content))
                        {
                            return null;
                        }
    
                        return JsonConvert.DeserializeObject<Metadata>(content);
                    }
                }
            }
    
            protected async Task TestSingleActionAsync(string authScheme, HttpStatusCode expectedStatusCode, Web.Models.Action action)
            {
                var urls = GenerateUrlsForAction(action);
    
                foreach (var url in urls)
                {
                    await TestSingleUrlAsync(authScheme, expectedStatusCode, action, url).ConfigureAwait(false);
                }
            }
    
            private async Task TestSingleUrlAsync(string authScheme, HttpStatusCode expectedStatusCode, Web.Models.Action action, string url)
            {
                foreach (var actionMethod in action.Methods)
                {
                    // Note: authentication is not used at the moment
                    //switch (authScheme)
                    //{
                    //    case "Basic":
                    //        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, CreateBasicAuth());
                    //        break;
                    //    case "Bearer":
                    //        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, CreateAccessToken());
                    //        break;
                    //}
    
                    using (var msg = new HttpRequestMessage(ActionMethodToHttpMethod(actionMethod), $"http://localhost/{url}"))
                    {
                        using (var response = await Client.SendAsync(msg).ConfigureAwait(false))
                        {
                            Console.WriteLine(expectedStatusCode != response.StatusCode
                                ? $"{actionMethod} {url} {expectedStatusCode} {response.StatusCode} NOK"
                                : $"{actionMethod} {url} {expectedStatusCode} {response.StatusCode} OK");
    
                            Assert.Equal(expectedStatusCode, response.StatusCode);
    
                            if (!response.IsSuccessStatusCode)
                            {
                                continue;
                            }
    
                            ValidateHeaders(response.Headers);
                        }
                    }
                }
            }
    
            private HttpMethod ActionMethodToHttpMethod(string actionMethod)
            {
                switch (actionMethod)
                {
                    case "GET": return HttpMethod.Get;
                    case "PUT": return HttpMethod.Put;
                    case "POST": return HttpMethod.Post;
                    case "DELETE": return HttpMethod.Delete;
                    default: throw new ArgumentOutOfRangeException(nameof(actionMethod));
                }
            }
    
            private IEnumerable<string> GenerateUrlsForAction(Web.Models.Action action)
            {
                var urls = new List<string>();
    
                var tokens = ParseTokens(action.Template);
    
                GenerateUrls(urls, tokens, action.Parameters, action.Template);
    
                return urls;
            }
    
            private void GenerateUrls(ICollection<string> urls, IReadOnlyCollection<string> tokens, IReadOnlyCollection<Parameter> actionParameters, string actionTemplate)
            {
                if (!tokens.Any())
                {
                    urls.Add(actionTemplate);
                    return;
                }
    
                var url = string.Empty;
                foreach (var token in tokens)
                {
                    url = GenerateUrlsForToken(token, actionParameters, string.IsNullOrEmpty(url) ? actionTemplate : url);
                }
    
                if (!string.IsNullOrEmpty(url))
                {
                    urls.Add(url);
                }
            }
    
            private static string GenerateUrlsForToken(string token, IEnumerable<Parameter> actionParameters, string actionTemplate)
            {
                var tokenName = token.Substring(1, token.Length - 2);
    
                var parameter = actionParameters.FirstOrDefault(a => a.Name == tokenName);
                if (parameter == null)
                {
                    return string.Empty;
                }
                switch (parameter.Type)
                {
                    case "System.String": return GenerateUrl(token, actionTemplate);
                    default: return string.Empty;
                }
            }
    
            private static string GenerateUrl(string token, string actionTemplate)
            {
                var val = GetValueForToken(token);
    
                var url = actionTemplate.Replace(token, val);
                return url;
            }
    
            private static string GetValueForToken(string token)
            {
                switch (token.ToLowerInvariant())
                {
                    case "{id}": return "1";
                    case "{countryName}": return "Finland";
                    default: return "1";
                }
            }
    
            private static List<string> ParseTokens(string actionTemplate)
            {
                var tokens = new List<string>();
    
                if (string.IsNullOrEmpty(actionTemplate))
                {
                    return tokens;
                }
    
                var tokenStart = actionTemplate.IndexOf('{');
                while (tokenStart >= 0)
                {
                    var tokenEnd = actionTemplate.IndexOf('}', tokenStart);
                    if (tokenEnd <= tokenStart)
                    {
                        break;
                    }
    
                    tokens.Add(actionTemplate.Substring(tokenStart, tokenEnd - tokenStart + 1));
    
                    tokenStart = actionTemplate.IndexOf('{', tokenEnd);
                }
    
                return tokens;
            }
        }
    }
    

### Test class

Actual test class doesn't do much, it just

- initializes mocked interfaces
- gets metadata by using base class
- foreach metadata actuib calls base class to execute request

Here is the implementation of test class:

    using Xunit;
    using System.Threading.Tasks;
    using System.Net;
    using Moq;
    using System.Collections.Generic;
    using Countries.Web.Models;
    
    namespace Countries.Tests.ControllerTests
    {
        public class AuthenticationTests : ControllerTestBase, IClassFixture<CustomWebApplicationFactory<Web.Startup>>
        {
            private readonly CustomWebApplicationFactory<Web.Startup> _factory;
    
            public AuthenticationTests(CustomWebApplicationFactory<Web.Startup> factory)
                : base(factory.CreateClient())
            {
                _factory = factory;
            }
    
            [Theory]
            [InlineData("", HttpStatusCode.Unauthorized)]
            public async Task Test(string authScheme, HttpStatusCode expectedStatusCode)
            {
                _factory.CountriesServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(new List<string>());
                _factory.CountriesServiceMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(new Country { Name = "Finland" });
    
                var metadata = await ReadMetadataAsync().ConfigureAwait(false);
    
                foreach (var action in metadata.Actions)
                {
                    await TestSingleActionAsync(authScheme, expectedStatusCode, action).ConfigureAwait(false);
                }
            }
        }
    }

When test is executed, if fails since authentication is not enabled in any of controller actions. This is expected result.