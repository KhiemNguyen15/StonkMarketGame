using FluentResults;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Core.Interfaces;

public interface IMarketDataProvider
{
    Task<Result<decimal>> GetPriceAsync(TickerSymbol ticker);
}