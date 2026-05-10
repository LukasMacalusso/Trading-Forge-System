using TraderForge.API.Requests;
using TraderForge.Application.DTOs.Queries;

namespace TraderForge.API.Mappers;

public static class PricesMapper
{
    public static GetMarketPricesQuery ToQuery(this GetMarketPricesRequest request)
    {
        return new GetMarketPricesQuery
        {
            Symbols = request.Symbols
        };
    }
}
