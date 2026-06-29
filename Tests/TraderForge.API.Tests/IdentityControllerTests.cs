using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Requests;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;

using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using TraderForge.Domain.Common;

namespace TraderForge.API.Tests;

public class IdentityControllerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ITraderRepository> _traderRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _planRepositoryMock;

    private readonly IdentityController _controller;

    public IdentityControllerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _traderRepositoryMock = new Mock<ITraderRepository>();
        _planRepositoryMock = new Mock<ISubscriptionPlanRepository>();

        
        _planRepositoryMock.Setup(x => x.GetByNameAsync("basic"))
            .ReturnsAsync(new SubscriptionPlan(
                Guid.NewGuid(), "Basic", 9.99m, 10000m, 2, 5, false));
                
        _identityServiceMock.Setup(x => x.RegisterNewAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success());
        
        var registerHandler = new RegisterTraderCommandHandler(
            _identityServiceMock.Object,
            _traderRepositoryMock.Object,
            _planRepositoryMock.Object,
            Mock.Of<IPublisher>() // <-- NUEVO
        );
        
        var loginHandler = new LoginTraderQueryHandler(_identityServiceMock.Object, _traderRepositoryMock.Object);
        _controller = new IdentityController(registerHandler, loginHandler);
    }

    [Fact]
    public async Task Register_WhenValidCommand_ReturnsOk()
    {
        var request = new RegisterTraderRequest
        { Email = "usertest@traderforge.com", Password = "fatdog1234" };

        var result = await _controller.Register(request);
        Assert.IsType<OkObjectResult>(result);
    }
}
