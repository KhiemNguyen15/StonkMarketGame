using Discord.Interactions;
using StonkMarketGame.Bot.Services;

namespace StonkMarketGame.Bot.Modules;

public class HelpModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly EmbedService _embedService;

    public HelpModule(EmbedService embedService)
    {
        _embedService = embedService;
    }

    [SlashCommand("help", "Display available commands and their usage")]
    public async Task Help()
    {
        var embed = _embedService.BuildHelp();
        await RespondAsync(embed: embed, ephemeral: true);
    }
}