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
    public decimal Current { get; set; }

    [JsonPropertyName("o")]
    public decimal Open { get; set; }

    [JsonPropertyName("h")]
    public decimal High { get; set; }

    [JsonPropertyName("l")]
    public decimal Low { get; set; }

    [JsonPropertyName("pc")]
    public decimal PreviousClose { get; set; }
}