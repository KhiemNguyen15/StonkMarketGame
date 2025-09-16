using FluentResults;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly IPortfolioRepository _portfolioRepository;

    public PortfolioService(IMarketDataProvider marketDataProvider, IPortfolioRepository portfolioRepository)
    {
        _marketDataProvider = marketDataProvider;
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Result> BuyAsync(ulong userId, TickerSymbol ticker, int quantity)
    {
        if (quantity <= 0)
            return Result.Fail("Quantity must be greater than zero.");

        var priceResult = await _marketDataProvider.GetPriceAsync(ticker);
        if (priceResult.IsFailed)
            return Result.Fail($"Could not get price for {ticker}.");

        var price = priceResult.Value;
        var totalCost = price * quantity;

        var portfolio = await _portfolioRepository.GetOrCreatePortfolioAsync(userId);

        if (portfolio.CashBalance < totalCost)
            return Result.Fail("Insufficient funds.");

        var holding = portfolio.GetHolding(ticker);
        if (holding is null)
        {
            holding = new StockHolding(ticker, quantity, price);
            portfolio.AddHolding(holding);
        }
        else
        {
            holding.AddShares(quantity, price);
        }

        portfolio.AdjustCash(-totalCost);

        var transaction = new Transaction(userId, ticker, TransactionType.Buy, quantity, price);
        await _portfolioRepository.SavePortfolioAndTransactionAsync(portfolio, transaction);

        return Result.Ok().WithSuccess($"Bought {quantity} shares of {ticker} at {price:C} for {totalCost:C}.");
    }

    public async Task<Result> SellAsync(ulong userId, TickerSymbol ticker, int quantity)
    {
        if (quantity <= 0)
            return Result.Fail("Quantity must be greater than zero.");

        var priceResult = await _marketDataProvider.GetPriceAsync(ticker);
        if (priceResult.IsFailed)
            return Result.Fail($"Could not get price for {ticker}.");

        var price = priceResult.Value;
        var portfolio = await _portfolioRepository.GetOrCreatePortfolioAsync(userId);

        var holding = portfolio.GetHolding(ticker);
        if (holding is null)
            return Result.Fail($"You do not own any shares of {ticker}.");

        if (holding.Quantity < quantity)
            return Result.Fail("You do not have enough shares to sell.");

        holding.RemoveShares(quantity);
        if (holding.Quantity <= 0)
            portfolio.RemoveHoldings(holding);

        var totalProceeds = price * quantity;
        portfolio.AdjustCash(totalProceeds);

        var transaction = new Transaction(userId, ticker, TransactionType.Sell, quantity, price);
        await _portfolioRepository.SavePortfolioAndTransactionAsync(portfolio, transaction);

        return Result.Ok().WithSuccess($"Sold {quantity} shares of {ticker} at {price:C} for {totalProceeds:C}.");
    }

    public async Task<Result<UserPortfolio>> GetPortfolioAsync(ulong userId)
    {
        var portfolio = await _portfolioRepository.GetOrCreatePortfolioAsync(userId);
        return Result.Ok(portfolio);
    }

    public async Task<Result<List<Transaction>>> GetTransactionHistoryAsync(ulong userId, int limit = 50)
    {
        try
        {
            var transactions = await _portfolioRepository.GetTransactionHistoryAsync(userId, limit);
            return Result.Ok(transactions);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve transaction history: {ex.Message}");
        }
    }
}