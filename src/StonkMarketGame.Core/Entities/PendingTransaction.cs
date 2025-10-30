using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Entities;

/// <summary>
/// Represents a pending transaction that will be executed when the market opens.
/// </summary>
public class PendingTransaction
{
    public string Id { get; private set; }
    public int ShortCode { get; private set; }
    public ulong UserId { get; private set; }
    public TickerSymbol Ticker { get; private set; }
    public TransactionType Type { get; private set; }
    public int Quantity { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime ScheduledFor { get; private set; }
    public PendingTransactionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }

    protected PendingTransaction()
    {
    }

    public PendingTransaction(
        ulong userId,
        TickerSymbol ticker,
        TransactionType type,
        int quantity,
        DateTime scheduledFor)
    {
        Id = Guid.NewGuid().ToString();
        UserId = userId;
        Ticker = ticker;
        Type = type;
        Quantity = quantity;
        RequestedAt = DateTime.UtcNow;
        ScheduledFor = scheduledFor;
        Status = PendingTransactionStatus.Pending;
    }

    public void MarkAsProcessed()
    {
        Status = PendingTransactionStatus.Processed;
    }

    public void MarkAsCancelled()
    {
        Status = PendingTransactionStatus.Cancelled;
    }

    public void MarkAsFailed(string? reason = null)
    {
        Status = PendingTransactionStatus.Failed;
        FailureReason = reason;
    }
}

public enum PendingTransactionStatus
{
    Pending,
    Processed,
    Cancelled,
    Failed
}
