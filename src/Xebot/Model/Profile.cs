namespace Xebot.Models;

public record Profile
{
    public required ulong Id { get; init; }
    public required string Name { get; set; }

    public required ulong TotalSeconds { get; set; }
}