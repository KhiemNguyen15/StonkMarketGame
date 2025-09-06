namespace StonkMarketGame.Core.Configuration;

public class ResilienceSettings
{
    public TimeoutSettings Timeout { get; set; } = new();
    public RetrySettings Retry { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
}

public class TimeoutSettings
{
    public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);
}

public class RetrySettings
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
    public string BackoffType { get; set; } = "Exponential";
}

public class CircuitBreakerSettings
{
    public double FailureRatio { get; set; } = 0.5;
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);
    public int MinimumThroughput { get; set; } = 3;
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);
}