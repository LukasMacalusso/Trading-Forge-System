using Moq;
using TraderForge.Application.DTOs.Queries;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Common;
using TraderForge.Domain.Services;
using Xunit;

namespace TraderForge.Application.Tests;

public class RefreshTraderTokenQueryHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly RefreshTraderTokenQueryHandler _handler;

    public RefreshTraderTokenQueryHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new RefreshTraderTokenQueryHandler(_identityServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Success_ReturnsTokenResponse()
    {
        var tokenResponse = new TokenResponse { AccessToken = "new-access", RefreshToken = "new-refresh" };
        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync("access", "refresh"))
            .ReturnsAsync(ResultGeneric<TokenResponse>.Success(tokenResponse));

        var result = await _handler.HandleAsync(new RefreshTraderTokenQuery
        {
            AccessToken = "access",
            RefreshToken = "refresh"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("new-access", result.Value!.AccessToken);
        Assert.Equal("new-refresh", result.Value.RefreshToken);
    }

    [Fact]
    public async Task HandleAsync_Failure_ReturnsFailure()
    {
        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultGeneric<TokenResponse>.Failure("Invalid tokens."));

        var result = await _handler.HandleAsync(new RefreshTraderTokenQuery
        {
            AccessToken = "bad",
            RefreshToken = "bad"
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid tokens.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_Exception_ReturnsFailure()
    {
        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _handler.HandleAsync(new RefreshTraderTokenQuery
        {
            AccessToken = "access",
            RefreshToken = "refresh"
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error", result.ErrorMessage);
    }
}
