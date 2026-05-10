using TraderForge.Application.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.DTOs.Responses;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetOrdersQueryHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(
        ITraderRepository traderRepository,
        IOrderRepository orderRepository)
    {
        _traderRepository = traderRepository;
        _orderRepository = orderRepository;
    }

    public async Task<ResultGeneric<List<OrderResponse>>> HandleAsync(GetOrdersQuery query)
    {
        try
        {
            var trader = await _traderRepository.GetByIdIncludePortfolioAsync(query.TraderId);
            if (trader == null)
                return ResultGeneric<List<OrderResponse>>.Failure("Trader not found.");

            var activePortfolio = trader.Portfolios.FirstOrDefault(p => p.IsActive);
            if (activePortfolio == null)
                return ResultGeneric<List<OrderResponse>>.Failure("No active portfolio found.");

            var orders = await _orderRepository.GetByPortfolioIdAsync(activePortfolio.Id);
            var response = orders.Select(OrderResponse.FromEntity).ToList();
            
            return ResultGeneric<List<OrderResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            return ResultGeneric<List<OrderResponse>>.Failure(ex.Message);
        }
    }
}
