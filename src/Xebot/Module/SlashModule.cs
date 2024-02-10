using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Xebot.Converter;
using Xebot.Storage;

namespace Xebot.Modules;

public class SlashModule(MemoryStorage _memoryStorage, ILogger<SlashModule> _logger) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("reset", "Reset all profiles and sessions")]
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    public async Task ResetData()
    {
        try
        {
            _memoryStorage.Profiles.Clear();
            _memoryStorage.ProfileSessions.Clear();
            await _memoryStorage.SaveStorageAsync();

            await RespondAsync($"{Context.User.Mention} All profiles and sessions have been reset.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error processing showall");
        }
    }

    // show all datas
    [SlashCommand("showactives", "List all active sessions")]
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    public async Task ShowActives()
    {
        try
        {
            var sessions = _memoryStorage.ProfileSessions.FindAll(x => x.DateEnd is null);
            var matchedItems = _memoryStorage.Profiles.Join(sessions, 
                item1 => item1.Id, 
                item2 => item2.ProfileId, 
                (item1, item2) => item1).AsEnumerable();

            var datas = string.Join(" \n ", matchedItems.Select(x => x.Name));

            await RespondAsync($"{Context.User.Mention} All active sessions : \n {datas}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error processing showactives");
        }
    }

    // show data of a profile
    [SlashCommand("show", "Show session data for one user")]
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    public async Task ShowUserData(SocketGuildUser user = null)
    {
        try
        {
            user ??= (SocketGuildUser) Context.User;
            var datas = _memoryStorage.GetProfile(user);
            
            await RespondAsync($@"{Context.User.Mention} 
- **Total Temps =** {DateConverter.ConvertSecondsToHumanHourReadable(datas.TotalSeconds)}  
- **Total Session =** {_memoryStorage.CountProfileSession(user.Id)}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"error processing pds for user {Context.Interaction.User.Id}");
        }
    }
}
