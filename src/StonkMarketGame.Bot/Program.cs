using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;
using StonkMarketGame.Bot;
using StonkMarketGame.Core.Interfaces;
using StonkMarketGame.Core.Services;
using StonkMarketGame.Infrastructure.MarketData;
using StonkMarketGame.Infrastructure.Persistence;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // Discord config
        services.Configure<DiscordSettings>(config.GetSection("Discord"));

        services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
        }));
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<InteractionHandler>();
        services.AddHostedService<BotService>();

        // EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // Refit
        services.AddRefitClient<IFinnhubApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://finnhub.io/api/v1/"));

        // Portfolio
        services.AddScoped<IMarketDataProvider, FinnhubMarketDataProvider>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();

        services.AddScoped<IPortfolioService, PortfolioService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();