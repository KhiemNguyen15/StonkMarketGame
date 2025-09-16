using FluentResults;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Interfaces;

public interface IPortfolioService
{
    Task<Result> BuyAsync(ulong userId, TickerSymbol ticker, int quantity);
    Task<Result> SellAsync(ulong userId, TickerSymbol ticker, int quantity);
    Task<Result<UserPortfolio>> GetPortfolioAsync(ulong userId);
    Task<Result<List<Transaction>>> GetTransactionHistoryAsync(ulong userId, int limit = 50);
}