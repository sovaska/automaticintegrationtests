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
