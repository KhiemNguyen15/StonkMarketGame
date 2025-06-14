using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Entities;

public class StockHolding
{
    public TickerSymbol Ticker { get; }
    public int Quantity { get; private set; }
    public decimal AveragePrice { get; private set; }

    public StockHolding(TickerSymbol ticker, int quantity, decimal avgPrice)
    {
        Ticker = ticker;
        Quantity = quantity;
        AveragePrice = avgPrice;
    }

    public void AddShares(int qty, decimal pricePerShare)
    {
        var totalCost = (Quantity * AveragePrice) + (qty * pricePerShare);
        Quantity += qty;
        AveragePrice = totalCost / Quantity;
    }

    public void RemoveShares(int qty)
    {
        if (qty > Quantity)
            throw new InvalidOperationException("Cannot sell more shares than owned.");

        Quantity -= qty;
    }
}