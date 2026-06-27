namespace TraderForge.Domain.Models;

public record Candlestick(
    long OpenTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    long CloseTime
);
