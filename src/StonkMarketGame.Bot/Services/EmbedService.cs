using Discord;
using StonkMarketGame.Core.DTOs;
using StonkMarketGame.Core.Entities;

namespace StonkMarketGame.Bot.Services;

public class EmbedService
{
    private static readonly Color BotColor = new(0x173488);

    public Embed BuildSuccess(string title, string description)
    {
        return new EmbedBuilder()
            .WithTitle($"✅ {title}")
            .WithDescription(description)
            .WithColor(Color.Green)
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game")
            .Build();
    }

    public Embed BuildError(string title, string description)
    {
        return new EmbedBuilder()
            .WithTitle($"❌ {title}")
            .WithDescription(description)
            .WithColor(Color.Red)
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game")
            .Build();
    }

    public Embed BuildInfo(string title, string description)
    {
        return new EmbedBuilder()
            .WithTitle($"{title}")
            .WithDescription(description)
            .WithColor(BotColor)
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
            .WithColor(BotColor)
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game");

        embed.AddField("💵 Cash Balance", $"`{cashBalance:C}`", inline: true);
        embed.AddField("📊 Holdings Value", $"`{holdingsValue:C}`", inline: true);

        if (holdings.Count != 0)
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

    public Embed BuildHistory(string username, List<Transaction> transactions)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"{username}'s Trading History")
            .WithColor(BotColor)
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game");

        if (transactions.Count == 0)
        {
            embed.WithDescription("You haven't made any trades yet. Use `/buy` to start trading!");
            return embed.Build();
        }

        embed.WithDescription($"Showing your last {Math.Min(transactions.Count, 50)} transactions");

        var groupedTransactions = transactions
            .Take(50)
            .GroupBy(t => t.Timestamp.Date)
            .OrderByDescending(g => g.Key)
            .Take(10);

        foreach (var group in groupedTransactions)
        {
            var dateHeader = group.Key.ToString("MMM dd, yyyy");
            var transactionsList = string.Join("\n", group
                .OrderByDescending(t => t.Timestamp)
                .Select(t =>
                {
                    var typeText = t.Type == TransactionType.Buy ? "BOUGHT" : "SOLD";
                    var unixTimestamp = new DateTimeOffset(t.Timestamp).ToUnixTimeSeconds();
                    return $"- `{typeText}` {t.Ticker.Value} x{t.Quantity} @ {t.Price:C} (<t:{unixTimestamp}:t>)";
                }));

            embed.AddField(dateHeader, transactionsList);
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
        embed.AddField("Change",
            $"{(quote.Change >= 0 ? "🟢" : "🔴")} {quote.Change:+0.##;-0.##} ({quote.PercentChange:+0.##;-0.##}%)",
            true);
        embed.AddField("Previous Close", $"`{quote.PreviousClose:C}`", true);

        embed.AddField("Day High", $"`{quote.High:C}`", true);
        embed.AddField("Day Low", $"`{quote.Low:C}`", true);
        embed.AddField("Open", $"`{quote.Open:C}`", true);

        return embed.Build();
    }

    /// <summary>
    /// Builds a Discord embed that lists a user's pending market orders.
    /// </summary>
    /// <param name="username">The display name shown in the embed title.</param>
    /// <param name="orders">The pending orders to display; up to 25 orders are shown and additional orders are noted.</param>
    /// <returns>An Embed containing the pending orders or a message that no pending orders exist.</returns>
    public Embed BuildPendingOrders(string username, List<PendingTransaction> orders)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"{username}'s Pending Orders")
            .WithColor(new Color(0xFFA500)) // Orange
            .WithCurrentTimestamp()
            .WithFooter("Stonk Market Game");

        if (orders.Count == 0)
        {
            embed.WithDescription("You have no pending orders.");
            return embed.Build();
        }

        embed.WithDescription($"You have {orders.Count} pending order{(orders.Count > 1 ? "s" : "")} scheduled for execution.");

        foreach (var order in orders.Take(25)) // Discord embed field limit
        {
            var typeText = order.Type == TransactionType.Buy ? "BUY" : "SELL";
            var scheduledTimestamp = new DateTimeOffset(order.ScheduledFor).ToUnixTimeSeconds();
            var requestedTimestamp = new DateTimeOffset(order.RequestedAt).ToUnixTimeSeconds();

            embed.AddField(
                $"{typeText} {order.Ticker.Value}",
                $"Quantity: `{order.Quantity}` shares\n" +
                $"Scheduled: <t:{scheduledTimestamp}:f>\n" +
                $"Requested: <t:{requestedTimestamp}:R>\n" +
                $"Order Code: `#{order.ShortCode}`",
                inline: false);
        }

        if (orders.Count > 25)
        {
            embed.AddField("\u200B", $"*...and {orders.Count - 25} more orders*", inline: false);
        }

        embed.WithDescription(
            embed.Description +
            "\n\nUse `/cancel-order <order-code>` to cancel a pending order.");

        return embed.Build();
    }

    /// <summary>
    /// Builds an informational embed listing available Stonk Market Game commands and usage examples.
    /// </summary>
    /// <returns>An Embed containing command descriptions, examples, and a getting-started guide for the game.</returns>
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
            "`/history` - View your trading history\n" +
            "`/buy <ticker> <quantity>` - Buy shares of a stock\n" +
            "`/sell <ticker> <quantity>` - Sell shares from your portfolio\n" +
            "`/pending` - View your pending orders (queued when market is closed)\n" +
            "`/cancel-order <order-code>` - Cancel a pending order\n" +
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