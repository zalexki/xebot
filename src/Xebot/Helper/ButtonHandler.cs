using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Xebot.Storage;

namespace Xebot.Helper
{
    public class ButtonHandler(
        MemoryStorage _memoryStorage,
        ILogger<ButtonHandler> _logger
    )
    {
        public async Task HandlePds(SocketMessageComponent component, ITextChannel _channelToSendEvents)
        {
            var utcNow = DateTime.UtcNow;
            var embed = new EmbedBuilder();

            try
            {
                // check if session is already started
                var alreadyHasSession = _memoryStorage.GetCurrentProfileSession(component.User.Id);
                if (alreadyHasSession is not null)
                {
                    await component.RespondAsync("Tu as déjà pris ton service.", ephemeral: true);
                    return;
                }
                var currentProfile = _memoryStorage.GetProfile(component.User);

                embed
                    .WithTitle($"{component.User.GlobalName} a pris son service")

                    // empty line
                    .AddField("** **", "** **")

                    .AddField("Début de service :", ConvertUtcToParisTime(utcNow))
                    .AddField("Temps total de service :", ConvertSecondsToHumanHourReadable(currentProfile.TotalSeconds))

                    // empty line
                    .AddField("** **", "** **")

                    .AddField("Bon service à toi !", "** **")
                    .WithColor(Color.Green);

                if (_channelToSendEvents is null)
                {
                    _logger.LogError("channel to send events is null");
                    await component.RespondAsync("ERROR: channel to send events is null");
                    return;
                }

                await _memoryStorage.StartProfileSession(component.User.Id, utcNow);
                await _channelToSendEvents.SendMessageAsync(
                    embed: embed.Build(), options: new RequestOptions() { Timeout = 25000, RetryMode = RetryMode.AlwaysRetry });

                await component.RespondAsync("Prise de service enregistré.", ephemeral: true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "channel to send embed is null");
            }
        }


        public async Task HandleFds(SocketMessageComponent component, ITextChannel _channelToSendEvents)
        {
            var utcNow = DateTime.UtcNow;
            var embed = new EmbedBuilder();

            // add check if session is has started
            var currentSession = _memoryStorage.GetCurrentProfileSession(component.User.Id);
            if (currentSession is null)
            {
                await component.RespondAsync("Tu n'as pas pris ton service.", ephemeral: true);
                return;
            }

            var profile = _memoryStorage.GetProfile(component.User);
            await _memoryStorage.EndProfileSession(component.User.Id, utcNow);

            // [nom d'affichage discord] a prit sa fin de service
            // Début de service : [heure à laquelle le bouton vert a été actionné]
            // Fin de service : [heure à laquelle le bouton rouge a été actionné]
            // Temps de service : [calcul du temps entre l'interaction avec le bouton vert et le bouton rouge]
            // Temps total de service : [addition du temps de service] Bonne fin de service à toi !
            embed
                // .WithTitle($"{component.User.GlobalName} a pris sa fin de service.")
                .AddField("▬▬▬▬▬▬▬▬▬▬ FDS ▬▬▬▬▬▬▬▬▬▬", "╰┈➤")
                .AddField($"** **", $"{component.User.Mention} a pris sa fin de service.")

                // empty line
                .AddField("**                                    **", "**                                   **")

                .AddField("Début de service :", ConvertUtcToParisTime(currentSession.DateStart), true)
                .AddField("Fin de service :", ConvertUtcToParisTime(currentSession.DateEnd ?? utcNow), true)
                .AddField("Temps de service :", ConvertSecondsToHumanHourReadable(currentSession.TotalSeconds), true)
                .AddField("Temps total de service :", ConvertSecondsToHumanHourReadable(profile.TotalSeconds), true)
                .AddField("Nombre total de service :", _memoryStorage.CountProfileSession(profile.Id))

                // empty line
                .AddField("** **", "** **")
                .AddField("Bonne fin de service à toi !", "** **")
                .WithColor(Color.Red);

            if (_channelToSendEvents is null)
            {
                _logger.LogError("channel to send embed is null");
                await component.RespondAsync("ERROR: channel not found");
                return;
            }

            await _channelToSendEvents.SendMessageAsync(
                embed: embed.Build(), options: new RequestOptions() { Timeout = 25000, RetryMode = RetryMode.AlwaysRetry });

            await component.RespondAsync("Fin de service enregistrée.", ephemeral: true);
        }


    }
}