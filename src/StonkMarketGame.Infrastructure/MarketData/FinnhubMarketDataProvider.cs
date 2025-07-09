using FluentResults;
using Microsoft.Extensions.Configuration;
using StonkMarketGame.Core.DTOs;
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

            if (response == null || response.Current <= 0)
                return Result.Fail($"No valid price returned for {ticker.Value}.");

            return Result.Ok(response.Current);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error fetching price for {ticker.Value}: {ex.Message}");
        }
    }

    public async Task<Result<StockQuote>> GetQuoteAsync(TickerSymbol ticker)
    {
        try
        {
            var response = await _api.GetQuoteAsync(ticker.Value, _apiKey);

            if (response == null || response.Current <= 0)
                return Result.Fail($"No quote found for {ticker.Value}.");

            var change = response.Current - response.PreviousClose;
            var percentChange = response.PreviousClose != 0 ? change / response.PreviousClose * 100 : 0;

            var quote = new StockQuote(
                Current: response.Current,
                Open: response.Open,
                High: response.High,
                Low: response.Low,
                PreviousClose: response.PreviousClose,
                Change: change,
                PercentChange: percentChange
            );

            return Result.Ok(quote);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error fetching quote for {ticker.Value}: {ex.Message}");
        }
    }
}