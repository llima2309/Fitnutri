using AppFitNutri.Core.Models;
using AppFitNutri.Core.Services.Login;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AppFitNutriTest
{
    public class ApiHttpTest
    {
        private readonly IApiHttp cut;
        public static class HttpResponseHelper
        {
            public static HttpResponseMessage CreateResponse<T>(T obj, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                return new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = JsonContent.Create(obj)
                };
            }
        }
        [Fact]
        public async Task PostAsyncLogin_DeveRetornarAuthResponse()
        {
            var authResponseFake = new AuthResponse("abc123", new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc));

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(HttpResponseHelper.CreateResponse(authResponseFake));

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.fit-nutri.com")
            };

            var cut = new ApiHttp(httpClient);

            var result = await cut.LoginAsync(
                new LoginRequest("luis", "Lf2309@@"),
                CancellationToken.None
            );

            var authResponse = await result.Content.ReadFromJsonAsync<AuthResponse>();

            Assert.NotNull(authResponse);
            Assert.Equal("abc123", authResponse.AccessToken);
        }

    }
}
