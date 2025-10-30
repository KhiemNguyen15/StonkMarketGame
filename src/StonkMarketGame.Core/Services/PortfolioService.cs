using FluentResults;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IMarketHoursValidator _marketHoursValidator;
    private readonly IPendingTransactionRepository _pendingTransactionRepository;

    public PortfolioService(
        IMarketDataProvider marketDataProvider,
        IPortfolioRepository portfolioRepository,
        IMarketHoursValidator marketHoursValidator,
        IPendingTransactionRepository pendingTransactionRepository)
    {
        _marketDataProvider = marketDataProvider;
        _portfolioRepository = portfolioRepository;
        _marketHoursValidator = marketHoursValidator;
        _pendingTransactionRepository = pendingTransactionRepository;
    }

    public async Task<Result> BuyAsync(ulong userId, TickerSymbol ticker, int quantity)
    {
        if (quantity <= 0)
            return Result.Fail("Quantity must be greater than zero.");

        // Check market hours
        if (_marketHoursValidator.ShouldEnforceMarketHours() && !_marketHoursValidator.IsMarketOpen())
        {
            var nextOpen = _marketHoursValidator.GetNextMarketOpen();
            if (nextOpen.HasValue)
            {
                // Validate funds at queue time and provide warning
                string warningMessage = "";
                var currentPriceResult = await _marketDataProvider.GetPriceAsync(ticker);
                if (currentPriceResult.IsSuccess)
                {
                    var currentPrice = currentPriceResult.Value;
                    var estimatedCost = currentPrice * quantity;
                    var userPortfolio = await _portfolioRepository.GetOrCreatePortfolioAsync(userId);

                    if (userPortfolio.CashBalance < estimatedCost)
                    {
                        warningMessage = $"\n\n⚠️ **Warning**: You currently have insufficient funds ({userPortfolio.CashBalance:C} available, {estimatedCost:C} needed at current price). Ensure you have enough when the market opens, or your order will fail.";
                    }
                }

                // Queue the transaction for next market open
                var pendingTransaction = new PendingTransaction(
                    userId,
                    ticker,
                    TransactionType.Buy,
                    quantity,
                    nextOpen.Value);

                await _pendingTransactionRepository.AddAsync(pendingTransaction);

                // Format timestamp for Discord
                var unixTimestamp = new DateTimeOffset(nextOpen.Value).ToUnixTimeSeconds();

                return Result.Ok().WithSuccess(
                    $"Market is closed. Your order to buy **{quantity:N0}** shares of **{ticker}** has been queued and will be executed when the market opens <t:{unixTimestamp}:F>.{warningMessage}");
            }
        }

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

        // Check market hours
        if (_marketHoursValidator.ShouldEnforceMarketHours() && !_marketHoursValidator.IsMarketOpen())
        {
            var nextOpen = _marketHoursValidator.GetNextMarketOpen();
            if (nextOpen.HasValue)
            {
                // Validate shares at queue time and provide warning
                string warningMessage = "";
                var userPortfolio = await _portfolioRepository.GetOrCreatePortfolioAsync(userId);
                var userHolding = userPortfolio.GetHolding(ticker);

                if (userHolding == null)
                {
                    warningMessage = $"\n\n⚠️ **Warning**: You do not currently own any shares of {ticker}. Your order will fail when the market opens.";
                }
                else if (userHolding.Quantity < quantity)
                {
                    warningMessage = $"\n\n⚠️ **Warning**: You only have {userHolding.Quantity:N0} shares but are trying to sell {quantity:N0}. Ensure you have enough when the market opens, or your order will fail.";
                }

                // Queue the transaction for next market open
                var pendingTransaction = new PendingTransaction(
                    userId,
                    ticker,
                    TransactionType.Sell,
                    quantity,
                    nextOpen.Value);

                await _pendingTransactionRepository.AddAsync(pendingTransaction);

                // Format timestamp for Discord
                var unixTimestamp = new DateTimeOffset(nextOpen.Value).ToUnixTimeSeconds();

                return Result.Ok().WithSuccess(
                    $"Market is closed. Your order to sell **{quantity:N0}** shares of **{ticker}** has been queued and will be executed when the market opens <t:{unixTimestamp}:F>.{warningMessage}");
            }
        }

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

    public async Task<Result<List<PendingTransaction>>> GetPendingOrdersAsync(ulong userId)
    {
        try
        {
            var pendingOrders = await _pendingTransactionRepository.GetUserPendingOrdersAsync(userId);
            return Result.Ok(pendingOrders.ToList());
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve pending orders: {ex.Message}");
        }
    }

    public async Task<Result> CancelPendingOrderAsync(ulong userId, int shortCode)
    {
        try
        {
            var order = await _pendingTransactionRepository.GetByShortCodeAsync(shortCode);

            if (order == null)
                return Result.Fail("Order not found.");

            if (order.UserId != userId)
                return Result.Fail("You do not have permission to cancel this order.");

            if (order.Status != PendingTransactionStatus.Pending)
                return Result.Fail(
                    "This order cannot be cancelled because it has already been processed or cancelled.");

            order.MarkAsCancelled();
            await _pendingTransactionRepository.UpdateAsync(order);

            return Result.Ok()
                .WithSuccess(
                    $"Successfully cancelled your order to {order.Type.ToString().ToLower()} {order.Quantity:N0} shares of {order.Ticker}.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to cancel order: {ex.Message}");
        }
    }
}