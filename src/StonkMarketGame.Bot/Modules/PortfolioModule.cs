using Discord.Interactions;
using Microsoft.Extensions.Logging;
using StonkMarketGame.Bot.Services;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Bot.Modules;

public class PortfolioModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IPortfolioService _portfolioService;
    private readonly EmbedService _embedService;
    private readonly ILogger<PortfolioModule> _logger;

    public PortfolioModule(IPortfolioService portfolioService, EmbedService embedService,
        ILogger<PortfolioModule> logger)
    {
        _portfolioService = portfolioService;
        _embedService = embedService;
        _logger = logger;
    }

    [SlashCommand("buy", "Buy shares of a stock")]
    public async Task Buy(string ticker, int quantity)
    {
        await DeferAsync();

        var result = await _portfolioService.BuyAsync(Context.User.Id, new TickerSymbol(ticker.ToUpper()), quantity);

        if (result.IsSuccess)
        {
            var embed = _embedService.BuildSuccess("Trade Successful", result.Successes.First().Message);
            await FollowupAsync(embed: embed);
        }
        else
        {
            _logger.LogWarning("Buy failed for user {UserId}, ticker {Ticker}, quantity {Quantity}: {Message}",
                Context.User.Id,
                ticker.ToUpper(),
                quantity,
                result.Errors.First().Message);

            var embed = _embedService.BuildError("Trade Failed", result.Errors.First().Message);
            await FollowupAsync(embed: embed);
        }
    }

    [SlashCommand("sell", "Sell shares of a stock")]
    public async Task Sell(string ticker, int quantity)
    {
        await DeferAsync();

        var result = await _portfolioService.SellAsync(Context.User.Id, new TickerSymbol(ticker.ToUpper()), quantity);

        if (result.IsSuccess)
        {
            var embed = _embedService.BuildSuccess("Trade Successful", result.Successes.First().Message);
            await FollowupAsync(embed: embed);
        }
        else
        {
            _logger.LogWarning("Sell failed for user {UserId}, ticker {Ticker}, quantity {Quantity}: {Message}",
                Context.User.Id,
                ticker.ToUpper(),
                quantity,
                result.Errors.First().Message);

            var embed = _embedService.BuildError("Trade Failed", result.Errors.First().Message);
            await FollowupAsync(embed: embed);
        }
    }

    [SlashCommand("portfolio", "View your portfolio and cash balance")]
    public async Task Portfolio()
    {
        await DeferAsync();

        var result = await _portfolioService.GetPortfolioAsync(Context.User.Id);

        if (result.IsFailed)
        {
            _logger.LogWarning("Portfolio lookup failed for user {UserId}", Context.User.Id);
            var errorEmbed = _embedService.BuildError("Action Failed", "Failed to load your portfolio");
            await FollowupAsync(embed: errorEmbed);
            return;
        }

        var portfolio = result.Value;

        var embed = _embedService.BuildPortfolio(
            Context.User.Username,
            portfolio.CashBalance,
            portfolio.Holdings.Sum(h => h.Quantity * h.AveragePrice),
            portfolio.Holdings.Select(h => (h.Ticker.Value, h.Quantity, h.AveragePrice)).ToList());

        await FollowupAsync(embed: embed);
    }

    [SlashCommand("history", "View your trading history")]
    public async Task History()
    {
        await DeferAsync();

        var result = await _portfolioService.GetTransactionHistoryAsync(Context.User.Id);

        if (result.IsFailed)
        {
            _logger.LogWarning("Transaction history lookup failed for user {UserId}", Context.User.Id);
            var errorEmbed = _embedService.BuildError("Action Failed", "Failed to load your transaction history");
            await FollowupAsync(embed: errorEmbed);
            return;
        }

        var transactions = result.Value;
        var embed = _embedService.BuildHistory(Context.User.Username, transactions);

        await FollowupAsync(embed: embed);
    }

    /// <summary>
    /// Displays the invoking user's pending orders, or informs them if none exist.
    /// </summary>
    /// <remarks>
    /// Sends an embed containing the list of pending orders, an informational embed when no orders are present, or an error embed if order retrieval fails.
    /// </remarks>
    [SlashCommand("pending", "View your pending orders")]
    public async Task Pending()
    {
        await DeferAsync();

        var result = await _portfolioService.GetPendingOrdersAsync(Context.User.Id);

        if (result.IsFailed)
        {
            _logger.LogWarning("Pending orders lookup failed for user {UserId}", Context.User.Id);
            var errorEmbed = _embedService.BuildError("Action Failed", "Failed to load your pending orders");
            await FollowupAsync(embed: errorEmbed);
            return;
        }

        var orders = result.Value;

        if (orders.Count == 0)
        {
            var embed = _embedService.BuildInfo("Pending Orders", "You have no pending orders.");
            await FollowupAsync(embed: embed);
            return;
        }

        var embed2 = _embedService.BuildPendingOrders(Context.User.Username, orders);
        await FollowupAsync(embed: embed2);
    }

    /// <summary>
    /// Cancels the calling user's pending order identified by the provided numeric order code.
    /// </summary>
    /// <param name="orderCode">The numeric code of the pending order to cancel (e.g., 123).</param>
    [SlashCommand("cancel-order", "Cancel a pending order")]
    public async Task CancelOrder(
        [Summary("order-code", "The order code (e.g., 123)")] int orderCode)
    {
        await DeferAsync();

        var result = await _portfolioService.CancelPendingOrderAsync(Context.User.Id, orderCode);

        if (result.IsSuccess)
        {
            var embed = _embedService.BuildSuccess("Order Cancelled", result.Successes.First().Message);
            await FollowupAsync(embed: embed);
        }
        else
        {
            _logger.LogWarning("Cancel order failed for user {UserId}, order code {OrderCode}: {Message}",
                Context.User.Id,
                orderCode,
                result.Errors.First().Message);

            var embed = _embedService.BuildError("Cancel Failed", result.Errors.First().Message);
            await FollowupAsync(embed: embed);
        }
    }
}