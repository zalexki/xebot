using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Xebot.Storage;

namespace Xebot.Modules;

public class SlashModule(MemoryStorage _memoryStorage) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("reset", "Reset all profiles and sessions.")]
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    public async Task ResetData()
    {
        _memoryStorage.Profiles.Clear();
        _memoryStorage.ProfileSessions.Clear();
        await _memoryStorage.SaveStorageAsync();

        await RespondAsync($"{Context.User.Mention} All profiles and sessions have been reset.", ephemeral: true);
    }

    // show all datas
    [SlashCommand("showall", "Show all data")]
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    public async Task ShowAllData()
    {
        var datas = $"{JsonConvert.SerializeObject(_memoryStorage.ProfileSessions, Formatting.Indented)}";

        await RespondAsync($"{datas} All profiles and sessions have been reset.", ephemeral: true);
    }

    // show data of a profile
    [SlashCommand("show", "Show all data")]
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    public async Task ShowUserData(SocketGuildUser user = null)
    {
        user ??= (SocketGuildUser)Context.User;
        var datas = $"{JsonConvert.SerializeObject(_memoryStorage.ProfileSessions.FindAll(x => x.ProfileId == user.Id), Formatting.Indented)}";

        await RespondAsync($"{Context.User.Mention} All profiles and sessions have been reset.", ephemeral: true);
        return;
    }
}
