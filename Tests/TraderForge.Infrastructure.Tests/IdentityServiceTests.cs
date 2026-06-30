using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Services;

namespace TraderForge.Infrastructure.Tests;

public class IdentityServiceTests
{
    private readonly Mock<IUserStore<Account>> _userStoreMock;
    private readonly UserManager<Account> _userManager;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ITraderRepository> _traderRepoMock;
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _userStoreMock = new Mock<IUserStore<Account>>();
        _userStoreMock.As<IUserPasswordStore<Account>>();
        _userStoreMock.As<IUserEmailStore<Account>>();
        _userStoreMock.As<IUserClaimStore<Account>>();
        var passwordHasher = new Mock<IPasswordHasher<Account>>();
        passwordHasher
            .Setup(x => x.HashPassword(It.IsAny<Account>(), It.IsAny<string>()))
            .Returns("hashed-password");
        _userManager = new UserManager<Account>(
            _userStoreMock.Object, null!, passwordHasher.Object, null!, null!, null!, null!, null!, null!);
        _configurationMock = new Mock<IConfiguration>();
        _traderRepoMock = new Mock<ITraderRepository>();
        _service = new IdentityService(_userManager, _configurationMock.Object, _traderRepoMock.Object);
    }

    [Fact]
    public async Task RegisterNewAccountAsync_Success_ReturnsSuccess()
    {
        _userStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.RegisterNewAccountAsync("user1", "test@test.com", "Pass123!");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterNewAccountAsync_Failure_ReturnsError()
    {
        var identityError = new IdentityError { Description = "Password too weak" };
        _userStoreMock
            .Setup(x => x.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var result = await _service.RegisterNewAccountAsync("user1", "test@test.com", "weak");

        Assert.False(result.IsSuccess);
        Assert.Contains("Password too weak", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAccountAsync_UserFound_ReturnsSuccess()
    {
        var account = new Account { Id = "user1", Email = "test@test.com" };
        _userStoreMock
            .Setup(x => x.FindByIdAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _userStoreMock
            .Setup(x => x.DeleteAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.DeleteAccountAsync("user1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAccountAsync_UserNotFound_ReturnsFailure()
    {
        _userStoreMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var result = await _service.DeleteAccountAsync("nonexistent");

        Assert.False(result.IsSuccess);
        Assert.Contains("Account not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAccountAsync_DeleteFails_ReturnsError()
    {
        var account = new Account { Id = "user1", Email = "test@test.com" };
        _userStoreMock
            .Setup(x => x.FindByIdAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _userStoreMock
            .Setup(x => x.DeleteAsync(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failed" }));

        var result = await _service.DeleteAccountAsync("user1");

        Assert.False(result.IsSuccess);
        Assert.Contains("Delete failed", result.ErrorMessage);
    }
}
