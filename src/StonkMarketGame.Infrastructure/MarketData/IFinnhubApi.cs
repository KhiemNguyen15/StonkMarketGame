using System.Text.Json.Serialization;
using Refit;

namespace StonkMarketGame.Infrastructure.MarketData;

public interface IFinnhubApi
{
    [Get("/quote")]
    Task<FinnhubQuoteResponse> GetQuoteAsync(
        [Query] string symbol,
        [Query] string token);
}

public record FinnhubQuoteResponse
{
    [JsonPropertyName("c")]
    public decimal CurrentPrice { get; set; }
}