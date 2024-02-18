using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Xebot.Models;

namespace Xebot.Storage;

public class MemoryStorage
{
    private const string _profilesPath = "profiles.json";
    private const string _profileSessionsPath = "profile_sessions.json";
    public readonly ulong ID_channelToSendEmbed = ulong.Parse(Environment.GetEnvironmentVariable("CHANNEL_TO_SEND_EMBED") ?? "0");
    public readonly ulong ID_channelToSendEvents = ulong.Parse(Environment.GetEnvironmentVariable("CHANNEL_TO_SEND_EVENTS") ?? "0");
    public ITextChannel channelToSendEvents;
    public readonly List<Profile> Profiles = [];
    public readonly List<ProfileSession> ProfileSessions = [];
    
    public readonly Converter.JsonConverter _converter;

    public MemoryStorage(Converter.JsonConverter converter)
    {
        _converter = converter;

        if (File.Exists(_profilesPath))
        {
            var json = File.ReadAllText(_profilesPath);
            Profiles = _converter.ToObject<List<Profile>>(json) ?? [];
        }

        if (File.Exists(_profileSessionsPath))
        {
            var json = File.ReadAllText(_profileSessionsPath);
            ProfileSessions = _converter.ToObject<List<ProfileSession>>(json) ?? [];
        }
    }

    public int CountProfileSession(ulong userid)
    {
        return ProfileSessions.Count(x => x.ProfileId == userid);
    }

    public Profile? GetProfile(ulong userID)
    {
        return Profiles.SingleOrDefault(x => x.Id == userID);
    }

    public Profile GetProfile(SocketUser user)
    {
        var profile = Profiles.SingleOrDefault(x => x.Id == user.Id);
        if (profile is null)
        {
            profile = new Profile
            {
                Id = user.Id,
                Name = user.Mention,
                TotalSeconds = 0
            };
            Profiles.Add(profile);
        }

        return profile;
    }

    public async Task AddProfile(ulong id, string name)
    {
        Profiles.Add(new Profile
        {
            Id = id,
            Name = name,
            TotalSeconds = 0
        });

        await SaveStorageAsync();
    }

    public ProfileSession? GetCurrentProfileSession(ulong profileId)
    {
        return ProfileSessions.SingleOrDefault(x => x.ProfileId == profileId && x.DateEnd is null);
    }

    public async Task StartProfileSession(ulong profileId, DateTime dateStart)
    {
        ProfileSessions.Add(new ProfileSession
        {
            ProfileId = profileId,
            DateStart = dateStart,
            DateEnd = null,
            TotalSeconds = 0
        });

        await SaveStorageAsync();
    }

    public async Task<bool> EndProfileSession(ulong profileId, DateTime dateEnd)
    {
        var currentSession = ProfileSessions.SingleOrDefault(x => x.ProfileId == profileId && x.DateEnd is null);
        if (currentSession is null) return false;

        currentSession.DateEnd = dateEnd;
        currentSession.TotalSeconds = (ulong) (dateEnd - currentSession.DateStart).TotalSeconds;
        
        var profile = Profiles.SingleOrDefault(x => x.Id == profileId);
        if (profile is null) return false;

        profile.TotalSeconds += currentSession.TotalSeconds;

        await SaveStorageAsync();
        return true;
    }

    public async Task SaveStorageAsync()
    {
        var json = _converter.FromObject(Profiles);
        await File.WriteAllTextAsync(_profilesPath, json);

        json = _converter.FromObject(ProfileSessions);
        await File.WriteAllTextAsync(_profileSessionsPath, json);
    }
}