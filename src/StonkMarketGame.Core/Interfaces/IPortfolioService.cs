using FluentResults;
using StonkMarketGame.Core.Entities;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Interfaces;

public interface IPortfolioService
{
    Task<Result> BuyAsync(ulong userId, TickerSymbol ticker, int quantity);
    Task<Result> SellAsync(ulong userId, TickerSymbol ticker, int quantity);
    /// <summary>
/// Retrieve the portfolio for the specified user.
/// </summary>
/// <param name="userId">The user's unique identifier.</param>
/// <returns>A Result containing the user's UserPortfolio on success, or failure details on error.</returns>
Task<Result<UserPortfolio>> GetPortfolioAsync(ulong userId);
    /// <summary>
/// Retrieves the recent transactions for the specified user.
/// </summary>
/// <param name="userId">The unique identifier of the user whose transaction history is requested.</param>
/// <param name="limit">Maximum number of transactions to return, up to the specified value; defaults to 50.</param>
/// <returns>A Result containing a list of Transaction objects (up to <paramref name="limit"/>) on success, or error information on failure.</returns>
Task<Result<List<Transaction>>> GetTransactionHistoryAsync(ulong userId, int limit = 50);
    /// <summary>
/// Retrieves the pending (unfulfilled) transactions for the specified user.
/// </summary>
/// <param name="userId">The identifier of the user whose pending orders to retrieve.</param>
/// <returns>A Result containing a list of PendingTransaction on success, or error information on failure.</returns>
Task<Result<List<PendingTransaction>>> GetPendingOrdersAsync(ulong userId);
    /// <summary>
/// Cancels a pending order belonging to the specified user using the order's numeric short code.
/// </summary>
/// <param name="userId">The identifier of the user who owns the pending order.</param>
/// <param name="shortCode">The numeric short code that identifies the pending order to cancel.</param>
/// <returns>A Result indicating success, or failure with error information.</returns>
Task<Result> CancelPendingOrderAsync(ulong userId, int shortCode);
}