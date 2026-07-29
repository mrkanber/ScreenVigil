namespace ScreenVigil.Models;

public sealed record UsageEntry(string Key, string Label, TimeSpan Duration);
