using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Authentication.Commands.AuthenticateUser;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Authentication;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Tests.TestHelpers;
using Xunit;
using DomainTask = TaskManagement.Domain.Entities.Task;
using SystemTask = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Unit.Application.Authentication.Commands;

/// <summary>
/// Unit tests for the AuthenticateUserCommandHandler.
/// </summary>
public class AuthenticateUserCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;
    private readonly Mock<UserDapperRepository> _mockUserQueryRepository;
    private readonly Mock<UserEfCommandRepository> _mockUserCommandRepository;
    private readonly Mock<TaskManagementDbContext> _mockContext;
    private readonly AuthenticateUserCommandHandler _handler;

    public AuthenticateUserCommandHandlerTests()
    {
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        
        // Create in-memory configuration that returns a connection string
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Data Source=:memory:"}
        };
        
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        _mockUserQueryRepository = new Mock<UserDapperRepository>(configuration);
        _mockContext = DbContextTestHelper.CreateMockDbContext();
        _mockUserCommandRepository = new Mock<UserEfCommandRepository>(_mockContext.Object);
        _handler = new AuthenticateUserCommandHandler(
            _mockAuthenticationService.Object,
            _mockUserQueryRepository.Object,
            _mockUserCommandRepository.Object,
            _mockContext.Object);
    }

    [Fact]
    public async SystemTask Handle_WithValidTokenAndExistingUser_ShouldReturnAuthenticationResponse()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim("oid", "azure-object-id")
        }));

        var existingUser = new User("test@example.com", "John", "Doe", "test-object-id");

        var jwtToken = "generated-jwt-token";

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)existingUser);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserCommandRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("test@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(jwtToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be(jwtToken);
        result.Value.ExpiresIn.Should().Be(3600);
        result.Value.User.Should().NotBeNull();
        result.Value.User.Email.Should().Be("test@example.com");
        result.Value.User.FirstName.Should().Be("John");
        result.Value.User.LastName.Should().Be("Doe");
    }

    [Fact]
    public async SystemTask Handle_WithValidTokenAndNewUser_ShouldCreateUserAndReturnAuthenticationResponse()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "newuser@example.com"),
            new Claim(ClaimTypes.GivenName, "Jane"),
            new Claim(ClaimTypes.Surname, "Smith"),
            new Claim("oid", "azure-object-id")
        }));

        var jwtToken = "generated-jwt-token";

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("newuser@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserCommandRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<User>());

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("newuser@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(jwtToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be(jwtToken);
        result.Value.ExpiresIn.Should().Be(3600);
        result.Value.User.Should().NotBeNull();
        result.Value.User.Email.Should().Be("newuser@example.com");
        result.Value.User.FirstName.Should().Be("Jane");
        result.Value.User.LastName.Should().Be("Smith");

        _mockUserCommandRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask Handle_WithInvalidToken_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "invalid-token"
        };

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Failure(AuthenticationErrors.InvalidAzureAdToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be(AuthenticationErrors.InvalidAzureAdToken.Code);
        result.Errors[0].Message.Should().Be(AuthenticationErrors.InvalidAzureAdToken.Message);
    }

    [Fact]
    public async SystemTask Handle_WithMissingEmailClaim_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim("oid", "azure-object-id")
            // Missing email claim
        }));

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be(AuthenticationErrors.EmailClaimMissing.Code);
        result.Errors[0].Message.Should().Be(AuthenticationErrors.EmailClaimMissing.Message);
    }

    [Fact]
    public async SystemTask Handle_WithJwtTokenGenerationFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim("oid", "azure-object-id")
        }));

        var existingUser = new User("test@example.com", "John", "Doe", "test-object-id");

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)existingUser);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserCommandRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("test@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(AuthenticationErrors.JwtTokenGenerationFailed));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be(AuthenticationErrors.JwtTokenGenerationFailed.Code);
        result.Errors[0].Message.Should().Be(AuthenticationErrors.JwtTokenGenerationFailed.Message);
    }

    [Fact]
    public async SystemTask Handle_ShouldIncludeAdditionalClaimsInJwtToken()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim("oid", "azure-object-id")
        }));

        var existingUser = new User("test@example.com", "John", "Doe", "test-object-id");

        var jwtToken = "generated-jwt-token";

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)existingUser);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserCommandRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("test@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(jwtToken));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockAuthenticationService.Verify(s => s.GenerateJwtTokenAsync(
            "test@example.com",
            It.Is<Dictionary<string, string>>(claims => 
                claims.ContainsKey("user_id") &&
                claims.ContainsKey("display_name") &&
                claims.ContainsKey("first_name") &&
                claims.ContainsKey("last_name")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask Handle_ShouldUpdateLastLoginForExistingUser()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim("oid", "azure-object-id")
        }));

        var existingUser = new User("test@example.com", "John", "Doe", "test-object-id");

        var jwtToken = "generated-jwt-token";

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)existingUser);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserCommandRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("test@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(jwtToken));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUserCommandRepository.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask Handle_WithNewUserWhoIsManager_ShouldSetRoleToManager()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "manager@example.com"),
            new Claim(ClaimTypes.GivenName, "Manager"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("oid", "azure-object-id")
        }));

        var jwtToken = "generated-jwt-token";
        User? createdUser = null;

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("manager@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserCommandRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, ct) => { createdUser = user; })
            .ReturnsAsync((User u, CancellationToken ct) => u);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUserCommandRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("manager@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(jwtToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdUser.Should().NotBeNull();
        createdUser!.Role.Should().Be(UserRole.Manager);
        _mockUserCommandRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUserCommandRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Role == UserRole.Manager), It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); // Once for Add, once for Update
    }

    [Fact]
    public async SystemTask Handle_WithExistingUserWhoIsManagerButHasEmployeeRole_ShouldUpdateRoleToManager()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "manager@example.com"),
            new Claim(ClaimTypes.GivenName, "Manager"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("oid", "azure-object-id")
        }));

        var existingUser = new User("manager@example.com", "Manager", "User", "test-object-id");
        // User is Employee by default

        var jwtToken = "generated-jwt-token";

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("manager@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)existingUser);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(existingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUserCommandRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("manager@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(jwtToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingUser.Role.Should().Be(UserRole.Manager);
        _mockUserCommandRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Role == UserRole.Manager), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask Handle_WithExistingUserWhoIsNoLongerManager_ShouldRevertRoleToEmployee()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "exmanager@example.com"),
            new Claim(ClaimTypes.GivenName, "Ex"),
            new Claim(ClaimTypes.Surname, "Manager"),
            new Claim("oid", "azure-object-id")
        }));

        var existingUser = new User("exmanager@example.com", "Ex", "Manager", "test-object-id");
        existingUser.UpdateRole(UserRole.Manager); // User was previously a manager

        var jwtToken = "generated-jwt-token";

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("exmanager@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)existingUser);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(existingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // User is no longer a manager

        _mockUserCommandRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("exmanager@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(jwtToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingUser.Role.Should().Be(UserRole.Employee);
        _mockUserCommandRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Role == UserRole.Employee), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask Handle_WithExistingAdminUserWhoIsNotManager_ShouldKeepAdminRole()
    {
        // Arrange
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = "valid-token"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.GivenName, "Admin"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("oid", "azure-object-id")
        }));

        var existingUser = new User("admin@example.com", "Admin", "User", "test-object-id");
        existingUser.UpdateRole(UserRole.Admin); // User is Admin

        var jwtToken = "generated-jwt-token";

        _mockAuthenticationService.Setup(s => s.ValidateAzureAdTokenAsync(command.AzureAdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimsPrincipal>.Success(claimsPrincipal));

        _mockUserQueryRepository.Setup(r => r.GetByEmailAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)existingUser);

        _mockUserQueryRepository.Setup(r => r.IsManagerAsync(existingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Admin is not a manager

        _mockUserCommandRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockAuthenticationService.Setup(s => s.GenerateJwtTokenAsync("admin@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(jwtToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingUser.Role.Should().Be(UserRole.Admin); // Admin role should remain unchanged
        _mockUserCommandRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Role == UserRole.Admin), It.IsAny<CancellationToken>()), Times.Once);
    }
}
