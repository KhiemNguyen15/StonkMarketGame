using Discord;
using StonkMarketGame.Core.DTOs;

namespace StonkMarketGame.Bot.Services;

public class EmbedService
{
    public Embed BuildSuccess(string title, string description)
    {
        return new EmbedBuilder()
            .WithTitle($"✅ {title}")
            .WithDescription(description)
            .WithColor(new Color(0x2ECC71)) // Green
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game")
            .Build();
    }

    public Embed BuildError(string title, string description)
    {
        return new EmbedBuilder()
            .WithTitle($"❌ {title}")
            .WithDescription(description)
            .WithColor(new Color(0xE74C3C)) // Red
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game")
            .Build();
    }

    public Embed BuildPortfolio(
        string username,
        decimal cashBalance,
        decimal holdingsValue,
        IReadOnlyCollection<(string ticker, int quantity, decimal avgPrice)> holdings)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"{username}'s Portfolio")
            .WithColor(new Color(0x173488))
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game");

        embed.AddField("💵 Cash Balance", $"`{cashBalance:C}`", inline: true);
        embed.AddField("📊 Holdings Value", $"`{holdingsValue:C}`", inline: true);

        if (holdings.Any())
        {
            embed.AddField("\u200B", "\u200B", inline: false);

            foreach (var holding in holdings)
            {
                embed.AddField(
                    $"{holding.ticker}",
                    $"{holding.quantity} shares\nAvg Price: `{holding.avgPrice:C}`",
                    inline: true);
            }
        }
        else
        {
            embed.AddField("📈 Holdings", "You don't own any stocks yet.", inline: false);
        }

        return embed.Build();
    }

    public Embed BuildStockEmbed(string ticker, StockQuote quote)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"📊 {ticker.ToUpper()} Stock Overview")
            .WithColor(quote.Change >= 0 ? Color.Green : Color.Red)
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game • Data from Finnhub");

        embed.AddField("Current Price", $"`{quote.Current:C}`", true);
        embed.AddField("Change", $"{(quote.Change >= 0 ? "🟢" : "🔴")} {quote.Change:+0.##;-0.##} ({quote.PercentChange:+0.##;-0.##}%)", true);
        embed.AddField("Previous Close", $"`{quote.PreviousClose:C}`", true);

        embed.AddField("Day High", $"`{quote.High:C}`", true);
        embed.AddField("Day Low", $"`{quote.Low:C}`", true);
        embed.AddField("Open", $"`{quote.Open:C}`", true);

        return embed.Build();
    }

    public Embed BuildHelp()
    {
        var embed = new EmbedBuilder()
            .WithTitle("📋 Stonk Market Game Commands")
            .WithDescription("Welcome to the Stonk Market Game! Here are the available commands:")
            .WithColor(new Color(0x3498DB)) // Blue
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game");

        embed.AddField("💹 Market Commands",
            "`/quote <ticker>` - Get real-time stock data\n" +
            "Example: `/quote AAPL`",
            inline: false);

        embed.AddField("💼 Portfolio Commands",
            "`/portfolio` - View your portfolio and cash balance\n" +
            "`/buy <ticker> <quantity>` - Buy shares of a stock\n" +
            "`/sell <ticker> <quantity>` - Sell shares from your portfolio\n" +
            "Example: `/buy MSFT 10`, `/sell AAPL 5`",
            inline: false);

        embed.AddField("ℹ️ Help",
            "`/help` - Display this help message",
            inline: false);

        embed.AddField("💡 Getting Started",
            "1. Check a stock price with `/quote`\n" +
            "2. Buy shares with `/buy`\n" +
            "3. Track your investments with `/portfolio`\n" +
            "4. Sell when you're ready with `/sell`",
            inline: false);

        return embed.Build();
    }
}