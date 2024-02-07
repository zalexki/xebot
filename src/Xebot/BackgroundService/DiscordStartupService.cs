using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xebot.Helper;

namespace Xebot.BackgroundService;

public class DiscordStartupService : IHostedService
{
    private readonly DiscordSocketClient _discord;
    private readonly ILogger<DiscordSocketClient> _logger;

    public DiscordStartupService(DiscordSocketClient discord, ILogger<DiscordSocketClient> logger)
    {
        _discord = discord;
        _logger = logger;

        _discord.Log += msg => LogHelper.OnLogAsync(_logger, msg);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("start discord");
        await _discord.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));
        await _discord.StartAsync();
        _logger.LogInformation("discord up");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discord.LogoutAsync();
        await _discord.StopAsync();
    }
}
