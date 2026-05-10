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

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("You must provide at least one symbol.", badRequest.Value);
    }

    [Fact]
    public async Task GetPrices_WhenSymbolsProvided_ReturnsOkWithPrices()
    {
        var prices = new Dictionary<string, decimal>
        {
            { "BTCUSDT", 6500 },
            { "ETHUSDT", 3400 },
        };

        _marketServiceMock
            .Setup(x => x.GetPricesAsync())
            .ReturnsAsync(prices);

        var request = new GetMarketPricesRequest
        { Symbols = ["BTCUSDT", "ETHUSDT"] };

        var result = await _controller.GetPrices(request);

        var okResult       = Assert.IsType<OkObjectResult>(result);
        var returnedPrices = Assert.IsType<Dictionary<string, decimal>>(okResult.Value);
        Assert.Equal(2, returnedPrices.Count);
        Assert.Equal(6500, returnedPrices["BTCUSDT"]);
        Assert.Equal(3400, returnedPrices["ETHUSDT"]);
    }

    [Fact]
    public async Task GetPrices_WhenHandlerReturnsFailure_ReturnsOkWithNull()
    {
        _marketServiceMock
            .Setup(x => x.GetPricesAsync())
            .ReturnsAsync(new Dictionary<string, decimal>());

        var request = new GetMarketPricesRequest
        { Symbols = ["BTCUSDT"] };

        var result = await _controller.GetPrices(request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Null(okResult.Value);
    }
}
