using System.Threading;
using System.Threading.Tasks;
using AppFitNutri.Models;
using AppFitNutri.Services;
using AppFitNutri.ViewModels;
using FluentAssertions;
using Xunit;

namespace AppFitNutriTests;

public sealed class AuthApiStubSuccess : IAuthApi
{
    public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct)
        => Task.FromResult(LoginResult.Success("jwt-token", System.DateTimeOffset.UtcNow.AddHours(1)));
}

public sealed class AuthApiStubFail : IAuthApi
{
    private readonly string _message;
    public AuthApiStubFail(string message) => _message = message;

    public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct)
        => Task.FromResult(LoginResult.Fail(_message));
}

public sealed class TokenStoreSpy : ITokenStore
{
    public string? SavedToken { get; private set; }
    public Task SaveAsync(string token, System.DateTimeOffset exp)
    {
        SavedToken = token;
        return Task.CompletedTask;
    }
    public Task<string?> GetAsync() => Task.FromResult<string?>(SavedToken);
    public Task ClearAsync() { SavedToken = null; return Task.CompletedTask; }
}

public class LoginViewModelTests
{
    [Fact]
    public async Task Entrar_Deve_Salvar_Token_Quando_Sucesso()
    {
        // Arrange
        var api = new AuthApiStubSuccess();
        var store = new TokenStoreSpy();
        var vm = new LoginViewModel(api, store)
        {
            EmailOrUserName = "user@fit.com",
            Password = "1234"
        };

        // Act
        await vm.EntrarCommand.ExecuteAsync(null);

        // Assert
        store.SavedToken.Should().NotBeNullOrEmpty();
        vm.ErrorMessage.Should().BeNull();
        vm.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task Entrar_Deve_Exibir_Mensagem_De_Erro_Quando_Falha()
    {
        // Arrange
        var api = new AuthApiStubFail("E-mail não verificado");
        var store = new TokenStoreSpy();
        var vm = new LoginViewModel(api, store)
        {
            EmailOrUserName = "user@fit.com",
            Password = "1234"
        };

        // Act
        await vm.EntrarCommand.ExecuteAsync(null);

        // Assert
        vm.ErrorMessage.Should().Be("E-mail não verificado");
        store.SavedToken.Should().BeNull();
    }

    [Fact]
    public async Task Entrar_Deve_Validar_Campos_Obrigatorios()
    {
        var api = new AuthApiStubSuccess();
        var store = new TokenStoreSpy();
        var vm = new LoginViewModel(api, store)
        {
            EmailOrUserName = "",
            Password = ""
        };

        await vm.EntrarCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("Informe login/e-mail e senha.");
        store.SavedToken.Should().BeNull();
    }
}
