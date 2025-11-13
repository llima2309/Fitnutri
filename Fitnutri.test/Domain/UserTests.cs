using System;
using Fitnutri.Domain;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Domain;

public class UserTests
{
    [Fact]
    public void User_ShouldAllowEmailConfirmation()
    {
        // Arrange
        var user = new User
        {
            UserName = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashedpassword",
            EmailVerificationCode = 123456
        };

        // Act
        user.EmailConfirmed = true;
        user.EmailVerificationCode = null;

        // Assert
        user.EmailConfirmed.Should().BeTrue();
        user.EmailVerificationCode.Should().BeNull();
    }

    [Fact]
    public void User_ShouldAllowStatusChanges()
    {
        // Arrange
        var user = new User
        {
            UserName = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashedpassword"
        };

        // Act
        user.Status = UserStatus.Approved;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedBy = "admin";

        // Assert
        user.Status.Should().Be(UserStatus.Approved);
        user.ApprovedAt.Should().NotBeNull();
        user.ApprovedBy.Should().Be("admin");
    }

    [Fact]
    public void User_ShouldAllowPasswordReset()
    {
        // Arrange
        var user = new User
        {
            UserName = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashedpassword"
        };

        var resetToken = "reset-token-123";
        var expiryDate = DateTime.UtcNow.AddHours(1);

        // Act
        user.PasswordResetToken = resetToken;
        user.PasswordResetExpiresAt = expiryDate;

        // Assert
        user.PasswordResetToken.Should().Be(resetToken);
        user.PasswordResetExpiresAt.Should().Be(expiryDate);
    }

    [Fact]
    public void User_ShouldAllowPerfilAssociation()
    {
        // Arrange
        var user = new User
        {
            UserName = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashedpassword"
        };
        var perfilId = Guid.NewGuid();

        // Act
        user.PerfilId = perfilId;

        // Assert
        user.PerfilId.Should().Be(perfilId);
    }
}
