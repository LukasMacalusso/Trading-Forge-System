using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;

using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using TraderForge.Domain.Common;

namespace TraderForge.API.Tests;

public class IdentityControllerLoginTests
{
    private static RegisterTraderCommandHandler CreateRegisterHandler()
    {
        return new RegisterTraderCommandHandler(
            Mock.Of<IIdentityService>(),
            Mock.Of<ITraderRepository>(),
            Mock.Of<ISubscriptionPlanRepository>());
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ReturnsOkWithToken()
    {
        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock
            .Setup(x => x.GetValidatedTokenAsync("test@test.com", "password"))
            .ReturnsAsync(ResultGeneric<string>.Success("jwt.token.here"));

        var loginHandler = new LoginTraderQueryHandler(
            identityServiceMock.Object, Mock.Of<ITraderRepository>());

        var controller = new IdentityController(CreateRegisterHandler(), loginHandler);

        var request = new LoginTraderRequest { Email = "test@test.com", Password = "password" };
        var result = await controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        var tokenProp = value!.GetType().GetProperty("token")?.GetValue(value);
        Assert.Equal("jwt.token.here", tokenProp);
    }

    [Fact]
    public async Task Login_WhenInvalidCredentials_ReturnsUnauthorized()
    {
        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock
            .Setup(x => x.GetValidatedTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ResultGeneric<string>.Failure("Invalid Credentials"));

        var loginHandler = new LoginTraderQueryHandler(
            identityServiceMock.Object, Mock.Of<ITraderRepository>());

        var controller = new IdentityController(CreateRegisterHandler(), loginHandler);

        var request = new LoginTraderRequest { Email = "wrong@test.com", Password = "wrong" };
        var result = await controller.Login(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var value = unauthorizedResult.Value;
        var errorProp = value!.GetType().GetProperty("error")?.GetValue(value);
        Assert.Equal("Invalid Credentials", errorProp);
    }

    [Fact]
    public async Task Register_WhenRegistrationFails_ReturnsBadRequest()
    {
        var identityServiceMock = new Mock<IIdentityService>();
        identityServiceMock
            .Setup(x => x.RegisterNewAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Failure("Password too short"));

        var registerHandler = new RegisterTraderCommandHandler(
            identityServiceMock.Object,
            Mock.Of<ITraderRepository>(),
            Mock.Of<ISubscriptionPlanRepository>());

        var controller = new IdentityController(
            registerHandler,
            new LoginTraderQueryHandler(Mock.Of<IIdentityService>(), Mock.Of<ITraderRepository>()));

        var request = new RegisterTraderRequest { Email = "test@test.com", Password = "short" };
        var result = await controller.Register(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var value = badRequest.Value;
        var errorProp = value!.GetType().GetProperty("error")?.GetValue(value);
        Assert.Contains("Password too short", (string)errorProp!);
    }
}
