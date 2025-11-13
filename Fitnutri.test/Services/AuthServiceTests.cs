using System;
using System.Threading;
using System.Threading.Tasks;
using Fitnutri.Application.Email;
using Fitnutri.Auth;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Fitnutri.test.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Setup InMemory Database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        // Setup mocks
        _mockEmailSender = new Mock<IEmailSender>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup JWT Options
        _jwtOptions = Options.Create(new JwtOptions
        {
            Key = "this-is-a-very-long-key-for-testing-purposes-32-bytes",
            Issuer = "FitNutri",
            Audience = "FitNutri-Users"
        });

        // Setup configuration mock
        _mockConfiguration.Setup(x => x["Auth:ResetPasswordBaseUrl"])
            .Returns("https://fit-nutri.com/reset-password");

        _authService = new AuthService(_context, _jwtOptions, _mockEmailSender.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenValidData()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";
        var password = "Password123!";

        // Act
        var user = await _authService.RegisterAsync(userName, email, password, CancellationToken.None);

        // Assert
        user.Should().NotBeNull();
        user.UserName.Should().Be(userName);
        user.Email.Should().Be(email.ToLowerInvariant());
        user.EmailConfirmed.Should().BeFalse();
        user.Status.Should().Be(UserStatus.Pending);
        user.Role.Should().Be(UserRole.User);

        // Verify password is hashed
        BCrypt.Net.BCrypt.Verify(password, user.PasswordHash).Should().BeTrue();

        // Verify user is saved to database
        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
    }

    [Theory]
    [InlineData("ab")] // too short
    [InlineData("user_123")] // invalid characters
    [InlineData("")] // empty
    public async Task RegisterAsync_ShouldThrowArgumentException_WhenInvalidUserName(string userName)
    {
        // Arrange
        var email = "test@test.com";
        var password = "Password123!";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.RegisterAsync(userName, email, password, CancellationToken.None));
        
        exception.Message.Should().Contain("Username inválido");
    }

    [Theory]
    [InlineData("password")] // no uppercase, number, special
    [InlineData("Password")] // no number, special
    [InlineData("Pass1!")] // too short
    [InlineData("")] // empty
    public async Task RegisterAsync_ShouldThrowArgumentException_WhenWeakPassword(string password)
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.RegisterAsync(userName, email, password, CancellationToken.None));
        
        exception.Message.Should().Contain("Senha fraca");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenUserNameExists()
    {
        // Arrange
        var userName = "existinguser";
        var email1 = "test1@test.com";
        var email2 = "test2@test.com";
        var password = "Password123!";

        // Create existing user
        await _authService.RegisterAsync(userName, email1, password, CancellationToken.None);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(userName, email2, password, CancellationToken.None));
        
        exception.Message.Should().Contain("Usuário ou e-mail já existe");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenEmailExists()
    {
        // Arrange
        var userName1 = "user1";
        var userName2 = "user2";
        var email = "test@test.com";
        var password = "Password123!";

        // Create existing user
        await _authService.RegisterAsync(userName1, email, password, CancellationToken.None);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(userName2, email, password, CancellationToken.None));
        
        exception.Message.Should().Contain("Usuário ou e-mail já existe");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUserAndToken_WhenValidCredentials()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";
        var password = "Password123!";

        // Register and approve user
        var user = await _authService.RegisterAsync(userName, email, password, CancellationToken.None);
        user.Status = UserStatus.Approved;
        user.EmailConfirmed = true;
        await _context.SaveChangesAsync();

        // Act
        var (loginUser, token, expiresAt) = await _authService.LoginAsync(userName, password, CancellationToken.None);

        // Assert
        loginUser.Should().NotBeNull();
        loginUser.Id.Should().Be(user.Id);
        token.Should().NotBeNullOrEmpty();
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ShouldLoginWithEmail_WhenValidCredentials()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";
        var password = "Password123!";

        // Register and approve user
        var user = await _authService.RegisterAsync(userName, email, password, CancellationToken.None);
        user.Status = UserStatus.Approved;
        user.EmailConfirmed = true;
        await _context.SaveChangesAsync();

        // Act - login with email instead of username
        var (loginUser, token, expiresAt) = await _authService.LoginAsync(email, password, CancellationToken.None);

        // Assert
        loginUser.Should().NotBeNull();
        loginUser.Id.Should().Be(user.Id);
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenUserNotFound()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.LoginAsync("nonexistent", "Password123!", CancellationToken.None));
        
        exception.Message.Should().Contain("Usuário ou email incorreto");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenWrongPassword()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";
        var password = "Password123!";
        var wrongPassword = "WrongPassword123!";

        var user = await _authService.RegisterAsync(userName, email, password, CancellationToken.None);
        user.Status = UserStatus.Approved;
        user.EmailConfirmed = true;
        await _context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.LoginAsync(userName, wrongPassword, CancellationToken.None));
        
        exception.Message.Should().Contain("Senha incorreta");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenUserNotApproved()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";
        var password = "Password123!";

        var user = await _authService.RegisterAsync(userName, email, password, CancellationToken.None);
        // User remains in Pending status

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.LoginAsync(userName, password, CancellationToken.None));
        
        exception.Message.Should().Contain("Usuário não aprovado");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenEmailNotConfirmed()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";
        var password = "Password123!";

        var user = await _authService.RegisterAsync(userName, email, password, CancellationToken.None);
        user.Status = UserStatus.Approved;
        // EmailConfirmed remains false
        await _context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.LoginAsync(userName, password, CancellationToken.None));
        
        exception.Message.Should().Contain("E-mail não verificado");
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldSendEmail_WhenUserExists()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";
        var password = "Password123!";

        var user = await _authService.RegisterAsync(userName, email, password, CancellationToken.None);
        user.Status = UserStatus.Approved;
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ForgotPasswordAsync(email, CancellationToken.None);

        // Assert
        result.Should().Contain("você receberá instruções");
        _mockEmailSender.Verify(x => x.SendAsync(
            It.Is<string>(e => e == email),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify reset token was set
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.PasswordResetToken.Should().NotBeNullOrEmpty();
        updatedUser.PasswordResetExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnSameMessage_WhenUserNotExists()
    {
        // Act
        var result = await _authService.ForgotPasswordAsync("nonexistent@test.com", CancellationToken.None);

        // Assert
        result.Should().Contain("você receberá instruções");
        _mockEmailSender.Verify(x => x.SendAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldResetPassword_WhenValidToken()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@test.com";
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";

        var user = await _authService.RegisterAsync(userName, email, oldPassword, CancellationToken.None);
        user.Status = UserStatus.Approved;
        user.PasswordResetToken = "valid-token";
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        // Act
        await _authService.ResetPasswordAsync("valid-token", newPassword, CancellationToken.None);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        BCrypt.Net.BCrypt.Verify(newPassword, updatedUser!.PasswordHash).Should().BeTrue();
        updatedUser.PasswordResetToken.Should().BeNull();
        updatedUser.PasswordResetExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrowException_WhenInvalidToken()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.ResetPasswordAsync("invalid-token", "NewPassword123!", CancellationToken.None));
        
        exception.Message.Should().Contain("Token inválido ou expirado");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrowException_WhenWeakPassword()
    {
        // Arrange
        var user = new User
        {
            UserName = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            PasswordResetToken = "valid-token",
            PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.ResetPasswordAsync("valid-token", "weak", CancellationToken.None));
        
        exception.Message.Should().Contain("Senha fraca");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
