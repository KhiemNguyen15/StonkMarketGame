using FluentResults;
using StonkMarketGame.Core.DTOs;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Interfaces;

public interface IMarketDataProvider
{
    Task<Result<decimal>> GetPriceAsync(TickerSymbol ticker);
    Task<Result<StockQuote>> GetQuoteAsync(TickerSymbol ticker);
}