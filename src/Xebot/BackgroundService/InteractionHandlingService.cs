using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xebot.Storage;

namespace Xebot.BackgroundService;

public class InteractionHandlingService(
    MemoryStorage _storage,
    DiscordSocketClient _discord,
    InteractionService _interactions,
    IServiceProvider _services,
    ILogger<InteractionService> _logger,
    Xebot.Converter.JsonConverter _converter
) : IHostedService
{

    private ITextChannel _channelToSendEmbed;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discord.Ready += () => _interactions.RegisterCommandsGloballyAsync(true);
        _discord.Ready += OnReady;
        _discord.ButtonExecuted += ButtonHandler;

        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task OnReady()
    {
        await SpawnMessage();

        var channel = await _discord.GetChannelAsync(1204419468164337714,
            options: new RequestOptions
            {
                RetryMode = RetryMode.AlwaysRetry,
                RatelimitCallback = RateLimitedCallbackGetChannel
            });

        if (channel is null)
        {
            _logger.LogError("failed to retrieve channel for {Id}", 1204419468164337714);
            return;
        }

        _channelToSendEmbed = channel as ITextChannel;
    }

    private async Task SpawnMessage()
    {
        var builder = new ComponentBuilder()
            .WithButton("PDS", "pds", ButtonStyle.Success)
            .WithButton("FDS", "fds", ButtonStyle.Danger);

        var channel = await _discord.GetChannelAsync(1204419468164337714,
            options: new RequestOptions
            {
                RetryMode = RetryMode.AlwaysRetry,
                RatelimitCallback = RateLimitedCallbackGetChannel
            });

        if (channel is null)
        {
            _logger.LogError("failed to retrieve channel for {Id}", 1204419468164337714);
            return;
        }

        var chanText = channel as ITextChannel;
        if (chanText is null) return;
         
        var message = await chanText.SendMessageAsync("bot is up", components: builder.Build());
    }

    public async Task ButtonHandler(SocketMessageComponent component)
    {
        var utcNow = DateTime.UtcNow;
        var embed = new EmbedBuilder();

        switch(component.Data.CustomId)
        {
            case "pds":
                embed
                    .WithTitle($"{component.User.GlobalName} a pris son service")

                    // empty line
                    .AddField("**                                    **", "**                                   **")

                    .AddField("Début de service :", ConvertUtcToParisTime(utcNow))
                    .AddField("Temps total de service :", "xxxxxx")
                    .AddField("Bon service à toi !", "** **")
                    .WithColor(Color.Green);

                await _channelToSendEmbed.SendMessageAsync(
                    embed: embed.Build(), 
                    options: new RequestOptions() { Timeout = 25000, RetryMode = RetryMode.AlwaysRetry });

                await component.RespondAsync();
            break;
            case "fds":
                // [nom d'affichage discord] a prit sa fin de service
                // Début de service : [heure à laquelle le bouton vert a été actionné]
                // Fin de service : [heure à laquelle le bouton rouge a été actionné]
                // Temps de service : [calcul du temps entre l'interaction avec le bouton vert et le bouton rouge]
                // Temps total de service : [addition du temps de service] Bonne fin de service à toi !
                embed
                    .WithTitle($"{component.User.GlobalName} a pris sa fin de service")

                    // empty line
                    .AddField("**                                    **", "**                                   **")

                    .AddField("Début de service :", "xx")
                    .AddField("Fin de service :", ConvertUtcToParisTime(utcNow))
                    .AddField("Temps de service :", "xxxxxx")
                    .AddField("Temps total de service :", "Bonne fin de service à toi !")
                    .AddField("Bonne fin de service à toi !", "** **")
                    .WithColor(Color.Red);

                await _channelToSendEmbed.SendMessageAsync(
                    embed: embed.Build(), 
                    options: new RequestOptions() { Timeout = 25000, RetryMode = RetryMode.AlwaysRetry });

                await component.RespondAsync();
            break;
        }
    }

    public async Task SaveStorageAsync()
    {
        var json = _converter.FromObject(_storage.Profiles);
        await File.WriteAllTextAsync("profiles.json", json);

        json = _converter.FromObject(_storage.ProfileSessions);
        await File.WriteAllTextAsync("profile_sessions.json", json);

        return;
    } 

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _interactions.Dispose();
        return Task.CompletedTask;
    }

    private async Task RateLimitedCallbackGetChannel(IRateLimitInfo rateLimitInfo)
    {
        _logger.LogWarning("rate limited GetChannel {infos}", JsonConvert.SerializeObject(rateLimitInfo, Formatting.Indented));
    }

    public static DateTime ConvertUtcToParisTime(DateTime utcDate)
    {
        TimeZoneInfo parisTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        DateTime parisTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, parisTimeZone);
        return parisTime;
    }
}