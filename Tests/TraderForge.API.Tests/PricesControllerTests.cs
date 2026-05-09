using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Requests;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Services;
namespace TraderForge.API.Tests;

public class PricesControllerTests
{
    private readonly PricesController _controller;
    private readonly Mock<IMarketService> _marketServiceMock;
    public PricesControllerTests()
    {
        _marketServiceMock = new Mock<IMarketService>();
        var handler = new GetMarketPricesQueryHandler(_marketServiceMock.Object);
        _controller = new PricesController(handler);
    }

    [Fact]
    public async Task GetPrices_WhenSymbolsListIsEmpty_ReturnsBadRequest()
    {
        var request = new GetMarketPricesRequest { Symbols = [] };
        var result = await _controller.GetPrices(request);
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
