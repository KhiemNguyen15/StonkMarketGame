namespace StonkMarketGame.Core.Configuration;

/// <summary>
/// Configuration settings for stock market trading hours validation.
/// </summary>
public sealed class MarketHoursSettings
{
    /// <summary>
    /// Whether to enforce market hours restrictions. When false, trading is allowed 24/7.
    /// Typically set to false for development/testing environments.
    /// </summary>
    public bool EnforceMarketHours { get; init; } = true;

    /// <summary>
    /// Market opening time in Eastern Time (9:30 AM ET).
    /// </summary>
    public TimeSpan MarketOpenTime { get; init; } = new(9, 30, 0);

    /// <summary>
    /// Market closing time in Eastern Time (4:00 PM ET).
    /// </summary>
    public TimeSpan MarketCloseTime { get; init; } = new(16, 0, 0);

    /// <summary>
    /// US market holidays (NYSE calendar) for the current year.
    /// Markets are closed on these dates regardless of day of week.
    /// Format: MM-dd (month-day without year).
    /// </summary>
    public List<string> MarketHolidays { get; init; } =
    [
        "01-01", // New Year's Day
        "01-20", // Martin Luther King Jr. Day (3rd Monday in January - 2025)
        "02-17", // Presidents' Day (3rd Monday in February - 2025)
        "04-18", // Good Friday (2025)
        "05-26", // Memorial Day (last Monday in May - 2025)
        "06-19", // Juneteenth National Independence Day
        "07-04", // Independence Day
        "09-01", // Labor Day (1st Monday in September - 2025)
        "11-27", // Thanksgiving Day (4th Thursday in November - 2025)
        "12-25"  // Christmas Day
    ];
}
