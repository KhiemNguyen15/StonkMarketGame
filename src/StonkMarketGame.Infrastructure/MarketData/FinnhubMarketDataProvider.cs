using FluentResults;
using Microsoft.Extensions.Configuration;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.ValueObjects;

namespace StonkMarketGame.Infrastructure.MarketData;

public class FinnhubMarketDataProvider : IMarketDataProvider
{
    private readonly IFinnhubApi _api;
    private readonly string _apiKey;

    public FinnhubMarketDataProvider(IFinnhubApi api, IConfiguration config)
    {
        _api = api;
        _apiKey = config["Finnhub:ApiKey"]
            ?? throw new InvalidOperationException("Finnhub API key missing in config.");
    }

    public async Task<Result<decimal>> GetPriceAsync(TickerSymbol ticker)
    {
        try
        {
            var response = await _api.GetQuoteAsync(ticker.Value, _apiKey);

            if (response == null || response.CurrentPrice <= 0)
                return Result.Fail($"No valid price returned for {ticker}.");

            return Result.Ok(response.CurrentPrice);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Finnhub API error: {ex.Message}");
        }
    }
}