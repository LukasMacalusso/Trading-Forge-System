using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;
using TraderForge.Domain.Repositories;
namespace TraderForge.Application.Tests;

public class RegisterTraderCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ITraderRepository> _traderRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _planRepositoryMock;
    private readonly RegisterTraderCommandHandler _handler;

    public RegisterTraderCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _traderRepositoryMock = new Mock<ITraderRepository>();
        _planRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _planRepositoryMock
            .Setup(x => x.GetByNameAsync("basic"))
            .ReturnsAsync(new SubscriptionPlan(Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false));
        _handler = new RegisterTraderCommandHandler(
            _identityServiceMock.Object,
            _traderRepositoryMock.Object,
            _planRepositoryMock.Object);
    }

    [Fact]
    public async Task RegisterTraderAsync_WhenValidCommand_ReturnsSuccessAndCreatesTrader()
    {
        var command = new RegisterTraderCommand
        {
            Email = "usertest@gmail.com", Password = "fatdog12345"
        };

        Result result = await _handler.RegisterTraderAsync(command);
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        _identityServiceMock.Verify(
            x => x.RegisterNewAccountAsync(It.IsAny<string>(), command.Email, command.Password), Times.Once);

        _traderRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Trader>(
                t =>
                t.Email == command.Email &&
                t.UserName == command.Email)),
            Times.Once);
    }
}
