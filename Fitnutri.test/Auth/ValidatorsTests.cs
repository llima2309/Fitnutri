using Fitnutri.Auth;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Auth;

public class ValidatorsTests
{
    [Theory]
    [InlineData("ab")]            // curto
    [InlineData("a_b")]           // underscore
    [InlineData("joao.silva")]    // ponto
    [InlineData("joao!")]         // especial
    [InlineData("com-espaco")]    // hífen
    public void Username_Invalido_Deve_Falhar(string username)
        => Validators.IsValidUserName(username).Should().BeFalse();

    [Theory]
    [InlineData("abc")]           // curto
    [InlineData("abcdefgh")]      // sem maiúscula, número, especial
    [InlineData("Abcdefgh")]      // sem número, especial
    [InlineData("Abcdefg1")]      // sem especial
    [InlineData("abcdefg1!")]     // sem maiúscula
    [InlineData("ABCDEFG1!")]     // sem minúscula
    public void Senha_Fracaa_Deve_Falhar(string pwd)
        => Validators.IsStrongPassword(pwd).Should().BeFalse();

    [Theory]
    [InlineData("Joao123")]       // 7 chars -> NÃO atende, só pra reforçar
    public void Senha_7_Deve_Falhar(string pwd)
        => Validators.IsStrongPassword(pwd).Should().BeFalse();

    [Fact]
    public void Senha_Forte_Deve_Passar()
        => Validators.IsStrongPassword("Strong!123").Should().BeTrue();

    [Theory]
    [InlineData("JoaoSilva")]
    [InlineData("joao123")]
    [InlineData("USER999")]
    public void Username_Valido_Deve_Passar(string username)
        => Validators.IsValidUserName(username).Should().BeTrue();
}
