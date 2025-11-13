using System;
using System.Text.Json;
using Fitnutri.Contracts;
using Fitnutri.Domain;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Contracts;

public class AuthDtosTests
{
    [Fact]
    public void RegisterRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var request = new RegisterRequest("testuser", "test@test.com", "Password123!");

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<RegisterRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.UserName.Should().Be("testuser");
        deserialized.Email.Should().Be("test@test.com");
        deserialized.Password.Should().Be("Password123!");
    }

    [Fact]
    public void LoginRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var request = new LoginRequest("testuser", "Password123!");

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<LoginRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.UserNameOrEmail.Should().Be("testuser");
        deserialized.Password.Should().Be("Password123!");
    }

    [Fact]
    public void AuthResponse_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var response = new AuthResponse(token, expiresAt);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<AuthResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.AccessToken.Should().Be(token);
        deserialized.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ConfirmEmailRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = 123456;
        var request = new ConfirmEmailRequest(userId, code);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<ConfirmEmailRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.UserId.Should().Be(userId);
        deserialized.Code.Should().Be(code);
    }

    [Fact]
    public void ConfirmEmailByIdentifierRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var request = new ConfirmEmailByIdentifierRequest("test@test.com", 123456);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<ConfirmEmailByIdentifierRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.EmailOrUsername.Should().Be("test@test.com");
        deserialized.Code.Should().Be(123456);
    }

    [Fact]
    public void ForgotPasswordRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var request = new ForgotPasswordRequest("test@test.com");

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<ForgotPasswordRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Email.Should().Be("test@test.com");
    }

    [Fact]
    public void ForgotPasswordResponse_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var message = "Se o e-mail existir em nossa base, você receberá instruções.";
        var response = new ForgotPasswordResponse(message);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<ForgotPasswordResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Message.Should().Be(message);
    }

    [Fact]
    public void ResetPasswordRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var token = "reset-token-123";
        var newPassword = "NewPassword123!";
        var request = new ResetPasswordRequest(token, newPassword);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<ResetPasswordRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Token.Should().Be(token);
        deserialized.NewPassword.Should().Be(newPassword);
    }

    [Fact]
    public void MeResponse_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userName = "testuser";
        var email = "test@test.com";
        var createdAt = DateTime.UtcNow;
        var emailConfirmed = true;
        var status = UserStatus.Approved;
        var response = new MeResponse(id, userName, email, createdAt, emailConfirmed, status);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<MeResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(id);
        deserialized.UserName.Should().Be(userName);
        deserialized.Email.Should().Be(email);
        deserialized.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        deserialized.EmailConfirmed.Should().Be(emailConfirmed);
        deserialized.Status.Should().Be(status);
    }

    [Fact]
    public void ApproveUserRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var request = new ApproveUserRequest("admin-user");

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<ApproveUserRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ApprovedBy.Should().Be("admin-user");
    }

    [Fact]
    public void RejectUserRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var request = new RejectUserRequest("admin-user", "Violação de termos");

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<RejectUserRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ApprovedBy.Should().Be("admin-user");
        deserialized.Reason.Should().Be("Violação de termos");
    }

    [Fact]
    public void AssociarPerfilRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var request = new AssociarPerfilRequest(PerfilTipo.Nutricionista);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<AssociarPerfilRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.TipoPerfil.Should().Be(PerfilTipo.Nutricionista);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RegisterRequest_ShouldAllowEmptyValues(string emptyValue)
    {
        // This test documents that the record allows empty/null values
        // Validation should happen at the service layer, not the DTO layer
        
        // Arrange & Act
        var request = new RegisterRequest(emptyValue, emptyValue, emptyValue);

        // Assert
        request.Should().NotBeNull();
        request.UserName.Should().Be(emptyValue);
        request.Email.Should().Be(emptyValue);
        request.Password.Should().Be(emptyValue);
    }

    [Fact]
    public void LoginRequest_ShouldTrimWhitespace_InServiceLayer()
    {
        // This test documents expected behavior - trimming should happen in service layer
        
        // Arrange
        var userNameWithSpaces = "  testuser  ";
        var request = new LoginRequest(userNameWithSpaces, "password");

        // Act & Assert
        request.UserNameOrEmail.Should().Be(userNameWithSpaces); // DTO preserves original input
        
        // Note: Trimming should be handled by AuthService.LoginAsync()
    }

    [Fact]
    public void AuthResponse_ShouldHandleUtcDates()
    {
        // Arrange
        var utcDate = DateTime.UtcNow.AddHours(1);
        var response = new AuthResponse("token", utcDate);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<AuthResponse>(json);

        // Assert
        deserialized!.ExpiresAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
