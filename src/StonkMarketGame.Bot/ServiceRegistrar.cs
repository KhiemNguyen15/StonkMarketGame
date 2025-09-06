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
        RegisterExternalApiServices(services);
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
    }

    private static void RegisterDatabaseServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }

    private static void RegisterExternalApiServices(IServiceCollection services)
    {
        services.AddRefitClient<IFinnhubApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://finnhub.io/api/v1/");
                c.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.MinimumThroughput = 3;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });
    }

    private static void RegisterCoreServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GameSettings>(configuration.GetSection("Game"));

        services.AddScoped<IMarketDataProvider, FinnhubMarketDataProvider>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IPortfolioService, PortfolioService>();
    }
}