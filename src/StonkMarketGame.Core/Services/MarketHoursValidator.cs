using StonkMarketGame.Core.Configuration;
using StonkMarketGame.Core.Interfaces;

namespace StonkMarketGame.Core.Services;

/// <summary>
/// Validates stock market trading hours based on NYSE/NASDAQ schedule.
/// Handles Eastern Time (ET) with DST transitions, weekends, and market holidays.
/// </summary>
public sealed class MarketHoursValidator : IMarketHoursValidator
{
    private static readonly TimeZoneInfo EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    private readonly MarketHoursSettings _settings;

    public MarketHoursValidator(MarketHoursSettings settings)
    {
        _settings = settings;
    }

    public bool ShouldEnforceMarketHours()
    {
        return _settings.EnforceMarketHours;
    }

    public bool IsMarketOpen()
    {
        if (!_settings.EnforceMarketHours)
        {
            return true; // Always open when enforcement is disabled
        }

        var nowEt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EasternTimeZone);

        // Check if it's a weekend
        if (nowEt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return false;
        }

        // Check if it's a market holiday
        var dateString = nowEt.ToString("MM-dd");
        if (_settings.MarketHolidays.Contains(dateString))
        {
            return false;
        }

        // Check if within market hours
        var currentTime = nowEt.TimeOfDay;
        return currentTime >= _settings.MarketOpenTime && currentTime < _settings.MarketCloseTime;
    }

    public DateTime? GetNextMarketOpen()
    {
        if (!_settings.EnforceMarketHours)
        {
            return null; // No next open time when enforcement is disabled
        }

        var nowEt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EasternTimeZone);
        var nextOpen = nowEt.Date.Add(_settings.MarketOpenTime);

        // If we're before market open today, and today is a trading day, return today's open
        if (nowEt.TimeOfDay < _settings.MarketOpenTime && IsTradingDay(nowEt.Date))
        {
            return TimeZoneInfo.ConvertTimeToUtc(nextOpen, EasternTimeZone);
        }

        // Otherwise, find the next trading day
        var candidateDate = nowEt.Date.AddDays(1);
        for (var i = 0; i < 10; i++) // Check up to 10 days ahead (handles long holiday weekends)
        {
            if (IsTradingDay(candidateDate))
            {
                nextOpen = candidateDate.Add(_settings.MarketOpenTime);
                return TimeZoneInfo.ConvertTimeToUtc(nextOpen, EasternTimeZone);
            }

            candidateDate = candidateDate.AddDays(1);
        }

        // Fallback: return next Monday at market open
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)nowEt.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        nextOpen = nowEt.Date.AddDays(daysUntilMonday).Add(_settings.MarketOpenTime);
        return TimeZoneInfo.ConvertTimeToUtc(nextOpen, EasternTimeZone);
    }

    private bool IsTradingDay(DateTime dateEt)
    {
        // Check if it's a weekend
        if (dateEt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return false;
        }

        // Check if it's a market holiday
        var dateString = dateEt.ToString("MM-dd");
        return !_settings.MarketHolidays.Contains(dateString);
    }
}