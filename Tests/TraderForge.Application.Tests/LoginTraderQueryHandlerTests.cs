using Moq;
using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;

namespace TraderForge.Application.Tests;

public class LoginTraderQueryHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ITraderRepository> _traderRepositoryMock;
    private readonly LoginTraderQueryHandler _handler;

    public LoginTraderQueryHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _traderRepositoryMock = new Mock<ITraderRepository>();
        _handler = new LoginTraderQueryHandler(_identityServiceMock.Object, _traderRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidCredentials_ReturnsToken()
    {
        const string expectedToken = "jwt.token.here";
        _identityServiceMock
            .Setup(x => x.GetValidatedTokenAsync("test@test.com", "password123"))
            .ReturnsAsync(ResultGeneric<string>.Success(expectedToken));

        var query = new LoginTraderQuery { Email = "test@test.com", Password = "password123" };
        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenInvalidCredentials_ReturnsFailure()
    {
        _identityServiceMock
            .Setup(x => x.GetValidatedTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultGeneric<string>.Failure("Invalid Credentials"));

        var query = new LoginTraderQuery { Email = "wrong@test.com", Password = "wrong" };
        var result = await _handler.HandleAsync(query);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid Credentials", result.ErrorMessage);
    }
}
