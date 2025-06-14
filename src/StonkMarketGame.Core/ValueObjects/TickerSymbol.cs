namespace StonkMarketGame.Core.ValueObjects;

public record TickerSymbol
{
    public string Value { get; }

    public TickerSymbol(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Ticker symbol cannot be empty.");

        Value = value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}