using TraderForge.Domain.Common;
using TraderForge.Application.DTOs;
using TraderForge.Application.DTOs.Responses;
using TraderForge.Domain.Repositories;

namespace TraderForge.Application.Handlers;

public class GetTransactionsQueryHandler
{
    private readonly ITraderRepository _traderRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsQueryHandler(
        ITraderRepository traderRepository,
        ITransactionRepository transactionRepository)
    {
        _traderRepository = traderRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<ResultGeneric<List<TransactionResponse>>> HandleAsync(GetTransactionsQuery query)
    {
        try
        {
            return await ExecuteAsync(query);
        }
        catch (Exception ex)
        {
            return ResultGeneric<List<TransactionResponse>>.Failure(ex.Message);
        }
    }

    private async Task<ResultGeneric<List<TransactionResponse>>> ExecuteAsync(GetTransactionsQuery query)
    {
        var trader = await _traderRepository.GetByIdIncludePortfolioAsync(query.TraderId);
        if (trader == null)
            return ResultGeneric<List<TransactionResponse>>.Failure("Trader not found.");

        var portfolio = query.PortfolioId.HasValue
            ? trader.Portfolios.FirstOrDefault(p => p.Id == query.PortfolioId.Value)
            : trader.Portfolios.FirstOrDefault(p => p.IsActive);

        if (portfolio == null)
            return ResultGeneric<List<TransactionResponse>>.Failure("Portfolio not found.");

        var transactions = await _transactionRepository.GetByPortfolioIdAsync(portfolio.Id);

        var response = transactions.Select(TransactionResponse.FromEntity).ToList();
        return ResultGeneric<List<TransactionResponse>>.Success(response);
    }
}
