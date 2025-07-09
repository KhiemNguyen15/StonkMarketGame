namespace StonkMarketGame.Core.DTOs;

public record StockQuote(
    decimal Current,
    decimal Open,
    decimal High,
    decimal Low,
    decimal PreviousClose,
    decimal Change,
    decimal PercentChange
);