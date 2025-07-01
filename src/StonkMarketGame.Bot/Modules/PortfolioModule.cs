using Discord;
using Discord.Interactions;
using StonkMarketGame.Bot.Services;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Bot.Modules;

public class PortfolioModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IPortfolioService _portfolioService;
    private readonly EmbedService _embedService;

    public PortfolioModule(IPortfolioService portfolioService, EmbedService embedService)
    {
        _portfolioService = portfolioService;
        _embedService = embedService;
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
}