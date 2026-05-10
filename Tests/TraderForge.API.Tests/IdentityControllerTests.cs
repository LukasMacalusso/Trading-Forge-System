using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;

namespace TraderForge.API.Tests;

public class IdentityControllerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ITraderRepository> _traderRepositoryMock;
    private readonly IdentityController _controller;

    public IdentityControllerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _traderRepositoryMock = new Mock<ITraderRepository>();
        var registerHandler = new RegisterTraderCommandHandler(_identityServiceMock.Object, _traderRepositoryMock.Object);
        var loginHandler = new LoginTraderQueryHandler(_identityServiceMock.Object, _traderRepositoryMock.Object);
        _controller = new IdentityController(registerHandler, loginHandler);
    }

    [Fact]
    public async Task Register_WhenValidCommand_ReturnsOk()
    {
        var command = new RegisterTraderCommand
        { Email = "usertest@traderforge.com", Password = "fatdog1234" };

        var result = await _controller.Register(command);
        Assert.IsType<OkObjectResult>(result);
    }
}
