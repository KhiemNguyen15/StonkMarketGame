using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StonkMarketGame.Bot.Services;

public class BotService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionHandler _interactionHandler;
    private readonly ILogger<BotService> _logger;
    private readonly string _token;

    public BotService(
        DiscordSocketClient client,
        InteractionHandler interactionHandler,
        ILogger<BotService> logger,
        IOptions<DiscordSettings> config)
    {
        _client = client;
        _interactionHandler = interactionHandler;
        _logger = logger;
        _token = config.Value.Token;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += msg =>
        {
            _logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        };

        _client.Ready += OnReadyAsync;

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        await _interactionHandler.InitializeAsync();

        _logger.LogInformation("Bot is starting...");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bot is stopping...");
        await _client.LogoutAsync();
        await _client.StopAsync();
    }

    private async Task OnReadyAsync()
    {
        await _client.SetGameAsync("with stonks 📈", type: ActivityType.Playing);
        _logger.LogInformation($"Bot connected as {_client.CurrentUser}");
        
        // Register commands globally
        await _interactionHandler.RegisterCommandsAsync();
    }
}