using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xebot.Storage;
using Xebot.Converter;
using Xebot.Models;
using System.Linq;

namespace Xebot.BackgroundService;

public class AutoEndService(ILogger<AutoEndService> _logger, MemoryStorage _memoryStorage) : Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {

        DoWork();

        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AutoEndService Hosted Service is stopping");
        }
    }
    private async void DoWork()
    {
        var list = _memoryStorage.ProfileSessions
            .FindAll(x => x.DateStart < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2)) & x.DateEnd == null);
            //.FindAll(x => x.DateStart < DateTime.UtcNow.Subtract(TimeSpan.FromHours(12)) & x.DateEnd == null);

        var utcNow = DateTime.UtcNow;
        foreach (var item in list)
        {
            await _memoryStorage.EndProfileSession(item.ProfileId, utcNow);
           
            var embed = new EmbedBuilder();

            var profile = _memoryStorage.GetProfile(item.ProfileId);
            embed
                .AddField("▬▬▬▬▬▬▬▬▬▬▬▬ Fin De Service ▬▬▬▬▬▬▬▬▬▬▬▬", "╰┈➤")
                .AddField($"** **", $"Automatique FDS pour {profile?.Name}, attention à bien prendre ta FDS la prochaine fois !")

                // empty line
                .AddField("** **", "** **")

                .AddField("Début de service :", DateConverter.ConvertUtcToParisTimeHumanReadable(item.DateStart), true)
                .AddField("Fin de service :", DateConverter.ConvertUtcToParisTimeHumanReadable(item.DateEnd ?? utcNow), true)
                .AddField("Temps de service :", DateConverter.ConvertSecondsToHumanHourReadable(item.TotalSeconds), true)
                .AddField("Temps total de service :", DateConverter.ConvertSecondsToHumanHourReadable(profile?.TotalSeconds ?? 0), true)
                .AddField("Nombre total de service :", _memoryStorage.CountProfileSession(item.ProfileId), true)

                // empty line
                .AddField("** **", "** **")
                .AddField("Bonne fin de service à toi !", "** **")
                .WithColor(Color.Red)
                .WithCurrentTimestamp();

            await _memoryStorage.channelToSendEvents.SendMessageAsync(embed: embed.Build());
        }
    }
}