using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fitnutri.Application.Services;
using Fitnutri.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Fitnutri.test.Services;

public class ViaCepServiceTests
{
    private readonly Mock<ILogger<ViaCepService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly ViaCepService _viaCepService;

    public ViaCepServiceTests()
    {
        _mockLogger = new Mock<ILogger<ViaCepService>>();
        _mockHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHandler.Object)
        {
            BaseAddress = new Uri("https://viacep.com.br/")
        };
        _viaCepService = new ViaCepService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAddressByCepAsync_ShouldReturnAddress_WhenValidCep()
    {
        // Arrange
        var cep = "01001-000";
        var expectedResponse = """
            {
                "cep": "01001-000",
                "logradouro": "Praça da Sé",
                "complemento": "lado ímpar",
                "unidade": "",
                "bairro": "Sé",
                "localidade": "São Paulo",
                "uf": "SP",
                "estado": "São Paulo",
                "regiao": "Sudeste",
                "ibge": "3550308",
                "gia": "1004",
                "ddd": "11",
                "siafi": "7107"
            }
            """;

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().NotBeNull();
        result!.CEP.Should().Be("01001-000");
        result.Logradouro.Should().Be("Praça da Sé");
        result.Complemento.Should().Be("lado ímpar");
        result.Bairro.Should().Be("Sé");
        result.Cidade.Should().Be("São Paulo");
        result.UF.Should().Be("SP");
        result.Estado.Should().Be("São Paulo");
        result.DDD.Should().Be("11");
    }

    [Theory]
    [InlineData("01001000")] // without dash
    [InlineData("01001-000")] // with dash
    [InlineData("01.001-000")] // with dot
    [InlineData("01 001 000")] // with spaces
    public async Task GetAddressByCepAsync_ShouldCleanCep_AndReturnAddress(string cep)
    {
        // Arrange
        var expectedResponse = """
            {
                "cep": "01001-000",
                "logradouro": "Praça da Sé",
                "bairro": "Sé",
                "localidade": "São Paulo",
                "uf": "SP",
                "estado": "São Paulo",
                "ddd": "11"
            }
            """;

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().NotBeNull();
        result!.CEP.Should().Be("01001-000");
        
        // Verify the correct URL was called (cleaned CEP)
        _mockHandler.Protected()
            .Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("01001000")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Theory]
    [InlineData("1234567")] // 7 digits
    [InlineData("123456789")] // 9 digits
    [InlineData("abcdefgh")] // non-numeric
    [InlineData("")] // empty
    [InlineData("   ")] // whitespace
    public async Task GetAddressByCepAsync_ShouldReturnNull_WhenInvalidCep(string invalidCep)
    {
        // Act
        var result = await _viaCepService.GetAddressByCepAsync(invalidCep);

        // Assert
        result.Should().BeNull();
        
        // Verify no HTTP request was made
        _mockHandler.Protected()
            .Verify("SendAsync", Times.Never(), 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetAddressByCepAsync_ShouldReturnNull_WhenCepNotFound()
    {
        // Arrange
        var cep = "99999-999";
        var errorResponse = """{"erro": true}""";

        SetupHttpResponse(HttpStatusCode.OK, errorResponse);

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressByCepAsync_ShouldReturnNull_WhenHttpError()
    {
        // Arrange
        var cep = "01001-000";
        SetupHttpResponse(HttpStatusCode.InternalServerError, "");

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressByCepAsync_ShouldReturnNull_WhenHttpException()
    {
        // Arrange
        var cep = "01001-000";
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressByCepAsync_ShouldHandleEmptyResponse()
    {
        // Arrange
        var cep = "01001-000";
        SetupHttpResponse(HttpStatusCode.OK, "");

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressByCepAsync_ShouldHandleInvalidJson()
    {
        // Arrange
        var cep = "01001-000";
        SetupHttpResponse(HttpStatusCode.OK, "invalid json");

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressByCepAsync_ShouldHandlePartialResponse()
    {
        // Arrange
        var cep = "01001-000";
        var partialResponse = """
            {
                "cep": "01001-000",
                "logradouro": "Praça da Sé",
                "localidade": "São Paulo",
                "uf": "SP"
            }
            """;

        SetupHttpResponse(HttpStatusCode.OK, partialResponse);

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().NotBeNull();
        result!.CEP.Should().Be("01001-000");
        result.Logradouro.Should().Be("Praça da Sé");
        result.Cidade.Should().Be("São Paulo");
        result.UF.Should().Be("SP");
        result.Bairro.Should().Be(""); // Empty when not provided
        result.Complemento.Should().BeNull(); // Null when not provided
        result.Estado.Should().Be(""); // Empty when not provided
        result.DDD.Should().Be(""); // Empty when not provided
    }

    [Fact]
    public async Task GetAddressByCepAsync_ShouldHandleNullFields()
    {
        // Arrange
        var cep = "01001-000";
        var responseWithNulls = """
            {
                "cep": "01001-000",
                "logradouro": null,
                "complemento": null,
                "bairro": null,
                "localidade": "São Paulo",
                "uf": "SP",
                "estado": null,
                "ddd": null
            }
            """;

        SetupHttpResponse(HttpStatusCode.OK, responseWithNulls);

        // Act
        var result = await _viaCepService.GetAddressByCepAsync(cep);

        // Assert
        result.Should().NotBeNull();
        result!.CEP.Should().Be("01001-000");
        result.Logradouro.Should().Be(""); // Null becomes empty string
        result.Complemento.Should().BeNull();
        result.Bairro.Should().Be(""); // Null becomes empty string
        result.Cidade.Should().Be("São Paulo");
        result.UF.Should().Be("SP");
        result.Estado.Should().Be(""); // Null becomes empty string
        result.DDD.Should().Be(""); // Null becomes empty string
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}
