namespace TraderForge.Domain.Events;

public record MarketPriceEvent(
    string Symbol,
    decimal Price,
    DateTime Timestamp
);
