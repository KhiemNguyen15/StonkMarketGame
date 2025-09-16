using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Entities;

public class Transaction
{
    public string Id { get; private set; }
    public ulong UserId { get; private set; }
    public TickerSymbol Ticker { get; private set; }
    public TransactionType Type { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime Timestamp { get; private set; }

    protected Transaction()
    {
    }

    public Transaction(ulong userId, TickerSymbol ticker, TransactionType type, int quantity, decimal price)
    {
        Id = Guid.NewGuid().ToString();
        UserId = userId;
        Ticker = ticker;
        Type = type;
        Quantity = quantity;
        Price = price;
        TotalAmount = quantity * price;
        Timestamp = DateTime.UtcNow;
    }
}

public enum TransactionType
{
    Buy,
    Sell
}