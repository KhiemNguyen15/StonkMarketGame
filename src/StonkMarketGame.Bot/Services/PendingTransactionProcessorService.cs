using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StonkMarketGame.Core.Interfaces;

namespace StonkMarketGame.Bot.Services;

/// <summary>
/// Background service that processes pending transactions when the market opens.
/// Checks every minute if the market is open and executes queued orders.
/// </summary>
public class PendingTransactionProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingTransactionProcessorService> _logger;
    private readonly DiscordSocketClient _discordClient;
    private readonly EmbedService _embedService;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    public PendingTransactionProcessorService(
        IServiceProvider serviceProvider,
        ILogger<PendingTransactionProcessorService> logger,
        DiscordSocketClient discordClient,
        EmbedService embedService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _discordClient = discordClient;
        _embedService = embedService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Pending Transaction Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingTransactionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending transactions");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("Pending Transaction Processor Service stopped");
    }

    private async Task ProcessPendingTransactionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var marketHoursValidator = scope.ServiceProvider.GetRequiredService<IMarketHoursValidator>();

        // Only process if market is open
        if (!marketHoursValidator.IsMarketOpen())
        {
            return;
        }

        var pendingRepo = scope.ServiceProvider.GetRequiredService<IPendingTransactionRepository>();
        var portfolioService = scope.ServiceProvider.GetRequiredService<IPortfolioService>();

        var pendingTransactions = await pendingRepo.GetPendingAsync(cancellationToken);

        if (pendingTransactions.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} pending transactions", pendingTransactions.Count);

        foreach (var pending in pendingTransactions)
        {
            try
            {
                // Execute the transaction at current market price
                var result = pending.Type == Core.Entities.TransactionType.Buy
                    ? await portfolioService.BuyAsync(pending.UserId, pending.Ticker, pending.Quantity)
                    : await portfolioService.SellAsync(pending.UserId, pending.Ticker, pending.Quantity);

                if (result.IsSuccess)
                {
                    // Mark as processed
                    pending.MarkAsProcessed();
                    await pendingRepo.UpdateAsync(pending, cancellationToken);

                    _logger.LogInformation(
                        "Processed pending {Type} order: User {UserId}, {Quantity} shares of {Ticker}",
                        pending.Type,
                        pending.UserId,
                        pending.Quantity,
                        pending.Ticker.Value);

                    // Send success notification
                    await SendDmToUserAsync(
                        pending.UserId,
                        _embedService.BuildSuccess(
                            "Order Executed",
                            result.Successes.First().Message));
                }
                else
                {
                    // Mark as failed with reason
                    var failureReason = result.Errors.First().Message;
                    pending.MarkAsFailed(failureReason);
                    await pendingRepo.UpdateAsync(pending, cancellationToken);

                    _logger.LogWarning(
                        "Failed to execute pending {Type} order: User {UserId}, {Quantity} shares of {Ticker}. Reason: {Reason}",
                        pending.Type,
                        pending.UserId,
                        pending.Quantity,
                        pending.Ticker.Value,
                        failureReason);

                    // Send failure notification
                    await SendDmToUserAsync(
                        pending.UserId,
                        _embedService.BuildError(
                            "Order Failed",
                            $"Your pending order to {pending.Type.ToString().ToLower()} **{pending.Quantity:N0}** shares of **{pending.Ticker}** failed to execute.\n\n**Reason**: {failureReason}"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception while processing pending {Type} order: User {UserId}, {Quantity} shares of {Ticker}",
                    pending.Type,
                    pending.UserId,
                    pending.Quantity,
                    pending.Ticker.Value);

                // Mark as failed with exception message
                pending.MarkAsFailed($"System error: {ex.Message}");
                await pendingRepo.UpdateAsync(pending, cancellationToken);

                // Send failure notification
                await SendDmToUserAsync(
                    pending.UserId,
                    _embedService.BuildError(
                        "Order Failed",
                        $"Your pending order to {pending.Type.ToString().ToLower()} **{pending.Quantity:N0}** shares of **{pending.Ticker}** failed due to a system error.\n\nPlease try placing the order again."));
            }
        }
    }

    private async Task SendDmToUserAsync(ulong userId, Embed embed)
    {
        try
        {
            var user = await _discordClient.GetUserAsync(userId);
            if (user != null)
            {
                await user.SendMessageAsync(embed: embed);
                _logger.LogInformation("Sent DM notification to user {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Could not find user {UserId} to send DM notification", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send DM to user {UserId}", userId);
        }
    }
}