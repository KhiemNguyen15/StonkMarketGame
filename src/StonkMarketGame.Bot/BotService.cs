using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StonkMarketGame.Bot;

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

        _client.Ready += OnReady;

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

    private async Task OnReady()
    {
        _logger.LogInformation($"Bot connected as {_client.CurrentUser}");

        // ---------------------- DEBUG ----------------------
        // Ensure commands and registered globally later
        var testGuildId = ulong.Parse("TEST_GUILD_ID");
        // ---------------------- DEBUG ----------------------

        await _interactionHandler.RegisterCommandsAsync(testGuildId);
    }
}