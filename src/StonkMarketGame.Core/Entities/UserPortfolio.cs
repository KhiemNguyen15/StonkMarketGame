using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Entities;

public class UserPortfolio
{
    public ulong UserId { get; }
    public decimal CashBalance { get; private set; }
    public List<StockHolding> Holdings { get; }

    public UserPortfolio(ulong userId, decimal initialBalance = 10000m)
    {
        UserId = userId;
        CashBalance = initialBalance;
        Holdings = new List<StockHolding>();
    }

    public StockHolding? GetHolding(TickerSymbol ticker) =>
        Holdings.FirstOrDefault(h => h.Ticker == ticker);

    public void AddHolding(StockHolding holding) =>
        Holdings.Add(holding);

    public void RemoveHoldings(StockHolding holding) =>
        Holdings.Remove(holding);

    public void AdjustCash(decimal amount)
    {
        CashBalance += amount;
    }
}