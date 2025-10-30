using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using StonkMarketGame.Bot.Services;
using StonkMarketGame.Core.Configuration;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.Services;
using StonkMarketGame.Infrastructure.MarketData;
using StonkMarketGame.Infrastructure.Persistence;

namespace StonkMarketGame.Bot;

public static class ServiceRegistrar
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterDiscordServices(services, configuration);
        RegisterDatabaseServices(services, configuration);
        RegisterExternalApiServices(services, configuration);
        RegisterCoreServices(services, configuration);
    }

    private static void RegisterDiscordServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DiscordSettings>(configuration.GetSection("Discord"));

        services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
        }));
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<InteractionHandler>();
        services.AddSingleton<EmbedService>();
        services.AddHostedService<BotService>();
        services.AddHostedService<PendingTransactionProcessorService>();
    }

    private static void RegisterDatabaseServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }

    private static void RegisterExternalApiServices(IServiceCollection services, IConfiguration configuration)
    {
        var resilienceSettings = configuration.GetSection("Resilience").Get<ResilienceSettings>() ?? new ResilienceSettings();
        
        services.AddRefitClient<IFinnhubApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://finnhub.io/api/v1/");
                c.Timeout = resilienceSettings.Timeout.HttpClientTimeout;
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = resilienceSettings.Retry.MaxRetryAttempts;
                options.Retry.BackoffType = resilienceSettings.Retry.BackoffType.Equals("Exponential", StringComparison.OrdinalIgnoreCase) 
                    ? DelayBackoffType.Exponential 
                    : DelayBackoffType.Linear;
                options.Retry.Delay = resilienceSettings.Retry.BaseDelay;
                options.Retry.OnRetry = args =>
                {
                    Console.WriteLine($"Finnhub API retry #{args.AttemptNumber + 1} after {args.RetryDelay.TotalMilliseconds}ms delay. Exception: {args.Outcome.Exception?.Message}");
                    return ValueTask.CompletedTask;
                };
                
                options.CircuitBreaker.FailureRatio = resilienceSettings.CircuitBreaker.FailureRatio;
                options.CircuitBreaker.SamplingDuration = resilienceSettings.CircuitBreaker.SamplingDuration;
                options.CircuitBreaker.MinimumThroughput = resilienceSettings.CircuitBreaker.MinimumThroughput;
                options.CircuitBreaker.BreakDuration = resilienceSettings.CircuitBreaker.BreakDuration;
                options.CircuitBreaker.OnOpened = _ =>
                {
                    Console.WriteLine($"Finnhub API circuit breaker opened. Break duration: {resilienceSettings.CircuitBreaker.BreakDuration.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                };
                options.CircuitBreaker.OnClosed = _ =>
                {
                    Console.WriteLine("Finnhub API circuit breaker closed");
                    return ValueTask.CompletedTask;
                };
                
                options.AttemptTimeout.Timeout = resilienceSettings.Timeout.AttemptTimeout;
                options.AttemptTimeout.OnTimeout = _ =>
                {
                    Console.WriteLine($"Finnhub API request timed out after {resilienceSettings.Timeout.AttemptTimeout.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                };
            });
    }

    private static void RegisterCoreServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GameSettings>(configuration.GetSection("Game"));
        services.Configure<ResilienceSettings>(configuration.GetSection("Resilience"));

        // Market hours configuration and validation
        var marketHoursSettings = configuration.GetSection("MarketHours").Get<MarketHoursSettings>() ?? new MarketHoursSettings();
        services.AddSingleton(marketHoursSettings);
        services.AddScoped<IMarketHoursValidator, MarketHoursValidator>();

        // Repositories
        services.AddScoped<IMarketDataProvider, FinnhubMarketDataProvider>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IPendingTransactionRepository, PendingTransactionRepository>();

        // Services
        services.AddScoped<IPortfolioService, PortfolioService>();
    }
}