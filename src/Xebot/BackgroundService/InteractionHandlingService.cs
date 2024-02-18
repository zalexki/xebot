using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Newtonsoft.Json;

using Xebot.Storage;

namespace Xebot.BackgroundService;

public class InteractionHandlingService(
    MemoryStorage _memoryStorage,
    DiscordSocketClient _discord,
    InteractionService _interactions,
    IServiceProvider _services,
    ILogger<InteractionHandlingService> _logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discord.Ready += () => _interactions.RegisterCommandsGloballyAsync(true);
        _discord.Ready += OnReady;

        _discord.InteractionCreated += OnInteractionAsync;

        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task OnReady()
    {
        var success = await SpawnEmbedWithButtons();

        // set channel for answers
        var channel = await _discord.GetChannelAsync(_memoryStorage.ID_channelToSendEvents,
            options: new RequestOptions
            {
                RetryMode = RetryMode.AlwaysRetry,
                RatelimitCallback = RateLimitedCallbackGetChannel
            });

        if (channel is not ITextChannel textChannel)
        {
            _logger.LogError("failed to retrieve textChannel for {Id}", _memoryStorage.channelToSendEvents);
            return;
        }

        _memoryStorage.channelToSendEvents = textChannel;
    }

    private async Task<bool> SpawnEmbedWithButtons()
    {
        var component = new ComponentBuilder()
            .WithButton("PDS", "pds", ButtonStyle.Success)
            .WithButton("FDS", "fds", ButtonStyle.Danger);

        var channel = await _discord.GetChannelAsync(_memoryStorage.ID_channelToSendEmbed, options: new RequestOptions { RetryMode = RetryMode.AlwaysRetry, RatelimitCallback = RateLimitedCallbackGetChannel });
        if (channel is not ITextChannel chanText)
        {
            _logger.LogError("failed to retrieve channel for {Id}", _memoryStorage.ID_channelToSendEmbed);
            return false;
        }

        var messageAlreadyExists = false;
        var messages = await chanText.GetMessagesAsync().FlattenAsync();
        foreach (var message in messages)
        {
            if (message.Author.Id == _discord.CurrentUser.Id)
            {
                messageAlreadyExists = true;
            }
        }

        if (messageAlreadyExists) return true;

        var embed = new EmbedBuilder();
        embed
            .WithTitle("Pointeuse EMS Pillbox Hill")
            .AddField("PDS", "Prise De Service")
            .AddField("FDS", "Fin De Service")
            .WithImageUrl("https://cdn.discordapp.com/attachments/1033184725973610517/1205247718864461844/hopital_pillbox.png?ex=65d7ad68&is=65c53868&hm=83264fecc7e56479be11d8933aec9a16023babc6e2c2bc9721a1e54dc8149032&")
            .WithColor(Color.DarkRed);

        await chanText.SendMessageAsync(embed: embed.Build(), components: component.Build());
        return true;
    }

    private async Task OnInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_discord, interaction);
            var result = await _interactions.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ToString());
        }
        catch
        {
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(msg => msg.Result.DeleteAsync());
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _memoryStorage.SaveStorageAsync();
        _interactions.Dispose();
    }

    private Task RateLimitedCallbackGetChannel(IRateLimitInfo rateLimitInfo)
    {
        _logger.LogWarning("rate limited GetChannel {infos}", JsonConvert.SerializeObject(rateLimitInfo, Formatting.Indented));
        return Task.CompletedTask;
    }
}