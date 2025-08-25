using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StonkMarketGame.Bot.Services;

namespace StonkMarketGame.Bot;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly ILogger<InteractionHandler> _logger;
    private readonly EmbedService _embedService;

    public InteractionHandler(
        DiscordSocketClient client,
        InteractionService interactions,
        IServiceProvider services,
        ILogger<InteractionHandler> logger,
        EmbedService embedService)
    {
        _client = client;
        _interactions = interactions;
        _services = services;
        _logger = logger;
        _embedService = embedService;
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

        _interactions.SlashCommandExecuted += async (command, context, result) =>
        {
            if (!result.IsSuccess)
            {
                _logger.LogWarning($"Slash command error: {result.Error} - {result.ErrorReason}");

                // Send immediate error response to user
                try
                {
                    var errorEmbed = _embedService.BuildError(
                        "Command Error",
                        "Something went wrong executing this command. Please try again.");

                    if (!context.Interaction.HasResponded)
                    {
                        await context.Interaction.RespondAsync(embed: errorEmbed, ephemeral: true);
                    }
                    else
                    {
                        await context.Interaction.FollowupAsync(embed: errorEmbed, ephemeral: true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send error response to user for slash command failure");
                }
            }
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

            // Send immediate error response to prevent timeout
            try
            {
                var errorEmbed = _embedService.BuildError(
                    "Interaction Error",
                    "Something went wrong processing this interaction. Please try again.");

                if (!interaction.HasResponded)
                {
                    await interaction.RespondAsync(embed: errorEmbed);
                }
                else
                {
                    await interaction.FollowupAsync(embed: errorEmbed);
                }
            }
            catch (Exception responseEx)
            {
                _logger.LogError(responseEx, "Failed to send error response to user");
            }
        }
    }
}