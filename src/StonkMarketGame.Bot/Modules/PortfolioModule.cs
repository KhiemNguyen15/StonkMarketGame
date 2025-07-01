using Discord;
using Discord.Interactions;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Bot.Modules;

public class PortfolioModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioModule(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    [SlashCommand("buy", "Buy shares of a stock")]
    public async Task Buy(string ticker, int quantity)
    {
        await DeferAsync(ephemeral: true);

        var result = await _portfolioService.BuyAsync(Context.User.Id, new TickerSymbol(ticker.ToUpper()), quantity);

        if (result.IsSuccess)
        {
            await FollowupAsync(result.Successes.First().Message, ephemeral: true);
        }
        else
        {
            await FollowupAsync($"❌ {result.Errors.First().Message}", ephemeral: true);
        }
    }

    [SlashCommand("sell", "Sell shares of a stock")]
    public async Task Sell(string ticker, int quantity)
    {
        await DeferAsync(ephemeral: true);

        var result = await _portfolioService.SellAsync(Context.User.Id, new TickerSymbol(ticker.ToUpper()), quantity);

        if (result.IsSuccess)
        {
            await FollowupAsync(result.Successes.First().Message, ephemeral: true);
        }
        else
        {
            await FollowupAsync($"❌ {result.Errors.First().Message}", ephemeral: true);
        }
    }

    [SlashCommand("portfolio", "View your portfolio and cash balance")]
    public async Task Portfolio()
    {
        await DeferAsync(ephemeral: true);

        var result = await _portfolioService.GetPortfolioAsync(Context.User.Id);

        if (result.IsFailed)
        {
            await FollowupAsync($"❌ Failed to load portfolio.", ephemeral: true);
            return;
        }

        var portfolio = result.Value;

        decimal totalHoldingsValue = portfolio.Holdings.Sum(h => h.Quantity * h.AveragePrice);

        var embed = new EmbedBuilder()
            .WithTitle($"{Context.User.Username}'s Portfolio")
            .WithColor(new Color(0x173488))
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithFooter(footer => footer.WithText("Stonk Market Game"));

        embed.AddField("💵 Cash Balance", $"`{portfolio.CashBalance:C}`", inline: true);
        embed.AddField("📊 Holdings Value", $"`{totalHoldingsValue:C}`", inline: true);

        if (portfolio.Holdings.Any())
        {
            embed.AddField("\u200B", "\u200B", inline: false);

            foreach (var holding in portfolio.Holdings)
            {
                embed.AddField(
                    $"{holding.Ticker} 🟢",
                    $"{holding.Quantity} shares\nAvg Price: `{holding.AveragePrice:C}`",
                    inline: true);
            }
        }
        else
        {
            embed.AddField("📈 Holdings", "You don't own any stocks yet.", inline: false);
        }

        embed.WithFooter("Stonk Market Game");

        await FollowupAsync(embed: embed.Build(), ephemeral: true);
    }
}