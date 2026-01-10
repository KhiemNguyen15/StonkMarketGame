using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StonkMarketGame.Bot.Services;

public class DiscordHealthCheck : IHealthCheck
{
    private readonly DiscordSocketClient _client;

    public DiscordHealthCheck(DiscordSocketClient client)
    {
        _client = client;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var isConnected = _client.ConnectionState == ConnectionState.Connected;

        if (isConnected)
        {
            var data = new Dictionary<string, object>
            {
                { "connectionState", _client.ConnectionState.ToString() },
                { "latency", _client.Latency }
            };

            if (_client.CurrentUser != null)
            {
                data.Add("username", _client.CurrentUser.Username);
                data.Add("guilds", _client.Guilds.Count);
            }

            return Task.FromResult(
                HealthCheckResult.Healthy("Discord bot is connected", data));
        }

        return Task.FromResult(
            HealthCheckResult.Unhealthy(
                $"Discord bot is not connected. State: {_client.ConnectionState}"));
    }
}
