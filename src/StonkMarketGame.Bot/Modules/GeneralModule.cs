using Discord.Interactions;

namespace StonkMarketGame.Bot.Modules;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check if the bot is alive.")]
    public async Task PingAsync()
    {
        await RespondAsync("Pong!");
    }
}