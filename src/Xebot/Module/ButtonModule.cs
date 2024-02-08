using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Xebot.Converter;
using Xebot.Storage;

namespace Xebot.Modules;

public class ButtonModule(MemoryStorage _memoryStorage, ILogger<ButtonModule> _logger) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("pds")]
    public async Task HandlePds()
    {
        var utcNow = DateTime.UtcNow;
        var embed = new EmbedBuilder();

        try
        {
            // check if session is already started
            var alreadyHasSession = _memoryStorage.GetCurrentProfileSession(Context.Interaction.User.Id);
            if (alreadyHasSession is not null)
            {
                await Context.Interaction.RespondAsync("Tu as déjà pris ton service.", ephemeral: true);
                return;
            }
            var currentProfile = _memoryStorage.GetProfile(Context.Interaction.User);

            embed
                .AddField("▬▬▬▬▬▬▬▬▬▬▬▬ Prise De Service ▬▬▬▬▬▬▬▬▬▬▬▬", "╰┈➤")
                .AddField($"** **", $"{Context.Interaction.User.Mention} a pris sa fin de service.")

                // empty line
                .AddField("** **", "** **")

                .AddField("Début de service :", DateConverter.ConvertUtcToParisTimeHumanReadable(utcNow))
                .AddField("Temps total de service :", DateConverter.ConvertSecondsToHumanHourReadable(currentProfile.TotalSeconds))

                // empty line
                .AddField("** **", "** **")

                .AddField("Bon service à toi !", "** **")
                .WithColor(Color.Green);

            await _memoryStorage.StartProfileSession(Context.Interaction.User.Id, utcNow);
            await _memoryStorage.channelToSendEvents.SendMessageAsync(
                embed: embed.Build(), options: new RequestOptions() { Timeout = 25000, RetryMode = RetryMode.AlwaysRetry });

            await Context.Interaction.RespondAsync("Prise de service enregistré.", ephemeral: true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error processing pds for user {Context.Interaction.User.Id}");
        }
    }

    [ComponentInteraction("fds")]
    public async Task HandleFds()
    {
        try
        {
            var utcNow = DateTime.UtcNow;
            var embed = new EmbedBuilder();

            // add check if session is has started
            var currentSession = _memoryStorage.GetCurrentProfileSession(Context.Interaction.User.Id);
            if (currentSession is null)
            {
                await Context.Interaction.RespondAsync("Tu n'as pas pris ton service.", ephemeral: true);
                return;
            }

            var profile = _memoryStorage.GetProfile(Context.Interaction.User);
            await _memoryStorage.EndProfileSession(Context.Interaction.User.Id, utcNow);


            embed
                .AddField("▬▬▬▬▬▬▬▬▬▬▬▬ Fin De Service ▬▬▬▬▬▬▬▬▬▬▬▬", "╰┈➤")
                .AddField($"** **", $"{Context.Interaction.User.Mention} a pris sa fin de service.")

                // empty line
                .AddField("**                                    **", "**                                   **")

                .AddField("Début de service :", DateConverter.ConvertUtcToParisTimeHumanReadable(currentSession.DateStart), true)
                .AddField("Fin de service :", DateConverter.ConvertUtcToParisTimeHumanReadable(currentSession.DateEnd ?? utcNow), true)
                .AddField("Temps de service :", DateConverter.ConvertSecondsToHumanHourReadable(currentSession.TotalSeconds), true)
                .AddField("Temps total de service :", DateConverter.ConvertSecondsToHumanHourReadable(profile.TotalSeconds), true)
                .AddField("Nombre total de service :", _memoryStorage.CountProfileSession(profile.Id), true)

                // empty line
                .AddField("** **", "** **")
                .AddField("Bonne fin de service à toi !", "** **")
                .WithColor(Color.Red);

            if (_memoryStorage.channelToSendEvents is null)
            {
                _logger.LogError("channel to send embed is null");
                await Context.Interaction.RespondAsync("ERROR: channel not found");
                return;
            }

            await _memoryStorage.channelToSendEvents.SendMessageAsync(
                embed: embed.Build(), options: new RequestOptions() { Timeout = 25000, RetryMode = RetryMode.AlwaysRetry });

            await Context.Interaction.RespondAsync("Fin de service enregistrée.", ephemeral: true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error processing fds for user {Context.Interaction.User.Id}");
        }
    }
}
