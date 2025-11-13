using Fitnutri.Auth;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Auth;

public class ValidatorsTests
{
    [Theory]
    [InlineData("abc", true)]
    [InlineData("ABC", true)]
    [InlineData("123", true)]
    [InlineData("user123", true)]
    [InlineData("User123", true)]
    [InlineData("testuser", true)]
    [InlineData("1234567890123456789012345678901", true)] // 31 chars
    [InlineData("12345678901234567890123456789012", true)] // 32 chars
    public void IsValidUserName_ShouldReturnTrue_ForValidUserNames(string userName, bool expected)
    {
        // Act
        var result = Validators.IsValidUserName(userName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("ab", false)] // muito curto (2 chars)
    [InlineData("123456789012345678901234567890123", false)] // muito longo (33 chars)
    [InlineData("user_123", false)] // underscore não permitido
    [InlineData("user-123", false)] // hífen não permitido
    [InlineData("user.123", false)] // ponto não permitido
    [InlineData("user@123", false)] // @ não permitido
    [InlineData("user 123", false)] // espaço não permitido
    [InlineData("", false)] // string vazia
    [InlineData("   ", false)] // apenas espaços
    [InlineData(null, false)] // null
    public void IsValidUserName_ShouldReturnFalse_ForInvalidUserNames(string userName, bool expected)
    {
        // Act
        var result = Validators.IsValidUserName(userName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Password1!", true)] // senha forte válida
    [InlineData("MyP@ssw0rd", true)] // senha forte válida
    [InlineData("Str0ng#Pass", true)] // senha forte válida
    [InlineData("Complex1$", true)] // senha forte válida
    [InlineData("Valid8&Password", true)] // senha forte válida
    [InlineData("Test123@", true)] // senha forte válida
    [InlineData("Abc123#def", true)] // senha forte válida
    public void IsStrongPassword_ShouldReturnTrue_ForStrongPasswords(string password, bool expected)
    {
        // Act
        var result = Validators.IsStrongPassword(password);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("password", false)] // sem maiúscula, número e especial
    [InlineData("PASSWORD", false)] // sem minúscula, número e especial
    [InlineData("Password", false)] // sem número e especial
    [InlineData("Password1", false)] // sem especial
    [InlineData("Password!", false)] // sem número
    [InlineData("password1!", false)] // sem maiúscula
    [InlineData("PASSWORD1!", false)] // sem minúscula
    [InlineData("Pass1!", false)] // muito curto (menos de 8 chars)
    [InlineData("", false)] // string vazia
    [InlineData("   ", false)] // apenas espaços
    [InlineData(null, false)] // null
    [InlineData("12345678", false)] // apenas números
    [InlineData("abcdefgh", false)] // apenas minúsculas
    [InlineData("ABCDEFGH", false)] // apenas maiúsculas
    [InlineData("!@#$%^&*", false)] // apenas especiais
    public void IsStrongPassword_ShouldReturnFalse_ForWeakPasswords(string password, bool expected)
    {
        // Act
        var result = Validators.IsStrongPassword(password);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidUserName_ShouldHandleEmptyAndWhitespace()
    {
        // Act & Assert
        Validators.IsValidUserName("").Should().BeFalse();
        Validators.IsValidUserName("   ").Should().BeFalse();
        Validators.IsValidUserName("\t").Should().BeFalse();
        Validators.IsValidUserName("\n").Should().BeFalse();
    }

    [Fact]
    public void IsStrongPassword_ShouldHandleEmptyAndWhitespace()
    {
        // Act & Assert
        Validators.IsStrongPassword("").Should().BeFalse();
        Validators.IsStrongPassword("   ").Should().BeFalse();
        Validators.IsStrongPassword("\t").Should().BeFalse();
        Validators.IsStrongPassword("\n").Should().BeFalse();
    }

    [Fact]
    public void IsValidUserName_EdgeCases()
    {
        // Exatamente nos limites
        var minValid = "abc"; // 3 chars
        var maxValid = new string('a', 32); // 32 chars
        var tooShort = "ab"; // 2 chars
        var tooLong = new string('a', 33); // 33 chars

        // Act & Assert
        Validators.IsValidUserName(minValid).Should().BeTrue();
        Validators.IsValidUserName(maxValid).Should().BeTrue();
        Validators.IsValidUserName(tooShort).Should().BeFalse();
        Validators.IsValidUserName(tooLong).Should().BeFalse();
    }

    [Fact]
    public void IsStrongPassword_EdgeCases()
    {
        // Exatamente no limite mínimo
        var minValid = "Abc123#!"; // 8 chars, todos os requisitos
        var tooShort = "Abc12#!"; // 7 chars

        // Act & Assert
        Validators.IsStrongPassword(minValid).Should().BeTrue();
        Validators.IsStrongPassword(tooShort).Should().BeFalse();
    }

    [Theory]
    [InlineData("Test123#ção")] // mistura com acentos
    public void IsStrongPassword_ShouldHandleInternationalCharacters(string password)
    {
        // Act
        var result = Validators.IsStrongPassword(password);

        // Assert
        // O regex atual não considera caracteres acentuados como letras válidas
        // Este teste documenta o comportamento atual
        result.Should().BeTrue(); // ou ajuste conforme necessário
    }
}
