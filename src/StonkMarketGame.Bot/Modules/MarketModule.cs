using Discord.Interactions;
using Microsoft.Extensions.Logging;
using StonkMarketGame.Bot.Services;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Bot.Modules;

public class MarketModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly EmbedService _embedService;
    private readonly ILogger<MarketModule> _logger;

    public MarketModule(IMarketDataProvider marketDataProvider, EmbedService embedService, ILogger<MarketModule> logger)
    {
        _marketDataProvider = marketDataProvider;
        _embedService = embedService;
        _logger = logger;
    }

    [SlashCommand("quote", "Get real-time data about a stock")]
    public async Task Quote(string ticker)
    {
        await DeferAsync();

        var symbol = new TickerSymbol(ticker.ToUpper());
        var result = await _marketDataProvider.GetQuoteAsync(symbol);

        if (result.IsFailed)
        {
            _logger.LogWarning("Quote lookup failed for {Ticker}", ticker.ToUpper());
            var errorEmbed = _embedService.BuildError("Lookup Failed", $"No data found for {ticker.ToUpper()}.");
            await FollowupAsync(embed: errorEmbed);
            return;
        }

        var embed = _embedService.BuildStockEmbed(ticker, result.Value);
        await FollowupAsync(embed: embed);
    }
}