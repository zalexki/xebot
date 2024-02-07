using System.Collections.Generic;
using Xebot.Models;

namespace Xebot.Storage;

public class MemoryStorage
{
    public readonly List<Profile> Profiles = [];
    public readonly List<ProfileSession> ProfileSessions = [];
}