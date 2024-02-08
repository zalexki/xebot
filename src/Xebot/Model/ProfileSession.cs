using System;

namespace Xebot.Models;

public record ProfileSession
{
    public required ulong ProfileId { get; init; }
    public required DateTime DateStart { get; init; }
    public required DateTime? DateEnd { get; set; }
    public required ulong TotalSeconds { get; set; }
}