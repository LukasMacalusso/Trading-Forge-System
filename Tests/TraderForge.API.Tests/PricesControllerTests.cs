using Microsoft.AspNetCore.Mvc;
using Moq;
using TraderForge.API.Controllers;
using TraderForge.API.Requests;
using TraderForge.Application.DTOs.Responses;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Constants;
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
        var cacheItem = new MarketPriceCacheItem
        {
            Prices = new Dictionary<string, decimal>
            {
                { "BTCUSDT", 6500 },
                { "ETHUSDT", 3400 },
            },
            LastUpdated = DateTime.UtcNow
        };

        _marketServiceMock
            .Setup(x => x.GetPricesAsync())
            .ReturnsAsync(cacheItem);

        var request = new GetMarketPricesRequest
        { Symbols = ["BTCUSDT", "ETHUSDT"] };
        
        var result = await _controller.GetPrices(request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<MarketPricesResponse>(okResult.Value);
        
        Assert.Equal(2, responseDto.Prices.Count);
        Assert.Equal(6500, responseDto.Prices["BTCUSDT"]);
        Assert.Equal(3400, responseDto.Prices["ETHUSDT"]);
    }

    [Fact]
    public async Task GetPrices_WhenHandlerReturnsFailure_ReturnsOkWithNull()
    {
        _marketServiceMock
            .Setup(x => x.GetPricesAsync())
            .ReturnsAsync(new MarketPriceCacheItem());

        var request = new GetMarketPricesRequest
        { Symbols = ["BTCUSDT"] };

        var result = await _controller.GetPrices(request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Null(okResult.Value);
    }
}
