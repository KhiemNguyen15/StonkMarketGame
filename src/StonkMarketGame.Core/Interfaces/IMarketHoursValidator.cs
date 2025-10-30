namespace StonkMarketGame.Core.Interfaces;

/// <summary>
/// Validates whether the stock market is currently open for trading.
/// </summary>
public interface IMarketHoursValidator
{
    /// <summary>
    /// Determines if the market is currently open for trading.
    /// </summary>
    /// <returns>True if the market is open; otherwise, false.</returns>
    bool IsMarketOpen();

    /// <summary>
    /// Gets the next date and time when the market will open.
    /// </summary>
    /// <returns>The next market open time in UTC, or null if enforcement is disabled.</returns>
    DateTime? GetNextMarketOpen();

    /// <summary>
    /// Determines if market hours enforcement is enabled.
    /// </summary>
    /// <returns>True if market hours should be enforced; otherwise, false.</returns>
    bool ShouldEnforceMarketHours();
}
