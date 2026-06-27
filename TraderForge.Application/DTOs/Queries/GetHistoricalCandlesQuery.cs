namespace TraderForge.Application.DTOs.Queries;

public record GetHistoricalCandlesQuery(string Symbol, string Interval, int Limit = 500);
