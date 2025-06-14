using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace StonkMarketGame.Bot;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly ILogger<InteractionHandler> _logger;

    public InteractionHandler(
        DiscordSocketClient client,
        InteractionService interactions,
        IServiceProvider services,
        ILogger<InteractionHandler> logger)
    {
        _client = client;
        _interactions = interactions;
        _services = services;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _interactions.AddModulesAsync(typeof(InteractionHandler).Assembly, _services);

        _client.InteractionCreated += HandleInteraction;

        _interactions.Log += msg =>
        {
            _logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        };

        _interactions.SlashCommandExecuted += (command, context, result) =>
        {
            if (!result.IsSuccess)
                _logger.LogWarning($"Slash command error: {result.Error} - {result.ErrorReason}");

            return Task.CompletedTask;
        };
    }

    public async Task RegisterCommandsAsync(ulong? testGuildId = null)
    {
        if (testGuildId.HasValue)
        {
            await _interactions.RegisterCommandsToGuildAsync(testGuildId.Value);
            _logger.LogInformation($"Registered slash commands to test guild: {testGuildId}");
        }
        else
        {
            await _interactions.RegisterCommandsGloballyAsync();
            _logger.LogInformation("Registered slash commands globally");
        }
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Interaction handling failed");

            // Acknowledge interaction to prevent timeout
            if (interaction.Type == Discord.InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async async =>
                    await interaction.RespondAsync("Something went wrong executing this command.")).Unwrap();
        }
    }
}