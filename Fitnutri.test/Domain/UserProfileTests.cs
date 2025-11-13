using System;
using Fitnutri.Domain;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Domain;

public class UserProfileTests
{
    [Fact]
    public void UserProfile_ShouldBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var profile = new UserProfile
        {
            NomeCompleto = "João da Silva",
            CPF = "12345678901",
            Genero = Genero.Masculino,
            DataNascimento = new DateTime(1990, 5, 15),
            CEP = "12345-678",
            Estado = Estado.SP,
            Endereco = "Rua Teste, 123",
            Numero = "123",
            Cidade = "São Paulo",
            UserId = Guid.NewGuid()
        };

        // Assert
        profile.NomeCompleto.Should().Be("João da Silva");
        profile.CPF.Should().Be("12345678901");
        profile.Genero.Should().Be(Genero.Masculino);
        profile.DataNascimento.Should().Be(new DateTime(1990, 5, 15));
        profile.CEP.Should().Be("12345-678");
        profile.Estado.Should().Be(Estado.SP);
        profile.Endereco.Should().Be("Rua Teste, 123");
        profile.Numero.Should().Be("123");
        profile.Cidade.Should().Be("São Paulo");
        profile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        profile.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void UserProfile_ShouldAllowOptionalFields()
    {
        // Arrange
        var profile = new UserProfile
        {
            NomeCompleto = "Maria Silva",
            CPF = "12345678901",
            Genero = Genero.Feminino,
            DataNascimento = new DateTime(1985, 3, 20),
            CEP = "12345-678",
            Estado = Estado.RJ,
            Endereco = "Av. Principal",
            Numero = "456",
            Cidade = "Rio de Janeiro",
            UserId = Guid.NewGuid()
        };

        // Act
        profile.RG = "12.345.678-9";
        profile.Telefone = "(11) 99999-9999";
        profile.CRN = "CRN-3/12345";
        profile.Complemento = "Apto 101";
        profile.Bairro = "Centro";
        profile.UF = "RJ";
        profile.IBGE = "3304557";
        profile.DDD = "21";
        profile.UpdatedAt = DateTime.UtcNow;

        // Assert
        profile.RG.Should().Be("12.345.678-9");
        profile.Telefone.Should().Be("(11) 99999-9999");
        profile.CRN.Should().Be("CRN-3/12345");
        profile.Complemento.Should().Be("Apto 101");
        profile.Bairro.Should().Be("Centro");
        profile.UF.Should().Be("RJ");
        profile.IBGE.Should().Be("3304557");
        profile.DDD.Should().Be("21");
        profile.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(Genero.Masculino)]
    [InlineData(Genero.Feminino)]
    [InlineData(Genero.Outro)]
    [InlineData(Genero.PrefiroNaoInformar)]
    public void UserProfile_ShouldAcceptAllGeneroOptions(Genero genero)
    {
        // Arrange & Act
        var profile = new UserProfile
        {
            NomeCompleto = "Teste",
            CPF = "12345678901",
            Genero = genero,
            DataNascimento = DateTime.Now.AddYears(-30),
            CEP = "12345-678",
            Estado = Estado.SP,
            Endereco = "Teste",
            Numero = "123",
            Cidade = "Teste",
            UserId = Guid.NewGuid()
        };

        // Assert
        profile.Genero.Should().Be(genero);
    }

    [Theory]
    [InlineData(Estado.AC)]
    [InlineData(Estado.SP)]
    [InlineData(Estado.RJ)]
    [InlineData(Estado.TO)]
    public void UserProfile_ShouldAcceptAllEstadoOptions(Estado estado)
    {
        // Arrange & Act
        var profile = new UserProfile
        {
            NomeCompleto = "Teste",
            CPF = "12345678901",
            Genero = Genero.Masculino,
            DataNascimento = DateTime.Now.AddYears(-30),
            CEP = "12345-678",
            Estado = estado,
            Endereco = "Teste",
            Numero = "123",
            Cidade = "Teste",
            UserId = Guid.NewGuid()
        };

        // Assert
        profile.Estado.Should().Be(estado);
    }

    [Fact]
    public void UserProfile_ShouldAllowUpdate()
    {
        // Arrange
        var profile = new UserProfile
        {
            NomeCompleto = "Nome Original",
            CPF = "12345678901",
            Genero = Genero.Masculino,
            DataNascimento = new DateTime(1990, 1, 1),
            CEP = "12345-678",
            Estado = Estado.SP,
            Endereco = "Endereço Original",
            Numero = "123",
            Cidade = "São Paulo",
            UserId = Guid.NewGuid()
        };

        // Act
        profile.NomeCompleto = "Nome Atualizado";
        profile.Endereco = "Novo Endereço";
        profile.UpdatedAt = DateTime.UtcNow;

        // Assert
        profile.NomeCompleto.Should().Be("Nome Atualizado");
        profile.Endereco.Should().Be("Novo Endereço");
        profile.UpdatedAt.Should().NotBeNull();
    }
}
