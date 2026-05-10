using Moq;
using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Tests;

public class RemovePortfolioAssetCommandHandlerTests
{
    private readonly Mock<IPortfolioAssetRepository> _assetRepoMock;
    private readonly RemovePortfolioAssetCommandHandler _handler;

    public RemovePortfolioAssetCommandHandlerTests()
    {
        _assetRepoMock = new Mock<IPortfolioAssetRepository>();
        _handler = new RemovePortfolioAssetCommandHandler(_assetRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenAssetExists_ShouldRemove()
    {
        var assetId = Guid.NewGuid();
        var asset = new PortfolioAsset(assetId, "BTCUSDT", 1m, 50000m, Guid.NewGuid());

        _assetRepoMock.Setup(x => x.GetByIdAsync(assetId)).ReturnsAsync(asset);

        var command = new RemovePortfolioAssetCommand { AssetId = assetId };
        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        _assetRepoMock.Verify(x => x.RemoveAsync(asset), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAssetNotFound_ReturnsFailure()
    {
        _assetRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PortfolioAsset?)null!);

        var command = new RemovePortfolioAssetCommand { AssetId = Guid.NewGuid() };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Portfolio asset not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _assetRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new RemovePortfolioAssetCommand { AssetId = Guid.NewGuid() };
        var result = await _handler.HandleAsync(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("Database error", result.ErrorMessage);
    }
}
