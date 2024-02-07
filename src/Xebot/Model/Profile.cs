namespace Xebot.Models;

public record Profile
{
    public required string Name { get; init; }

    public required ulong TotalSeconds { get; init; }
}