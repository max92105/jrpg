namespace Jrpg.Core.Cinematics;

/// <summary>
/// One subtitle of a still's sequence: its line and how long it shows, so the lines follow a voice-over. An empty line
/// shows nothing for its time: a pause between two lines.
/// </summary>
public sealed record StillCaption
{
    /// <summary>The line shown; empty for a pause.</summary>
    public required string Text { get; init; }

    /// <summary>How long the line shows before the next one takes its place; above 0.</summary>
    public required double Seconds { get; init; }

    public TimeSpan Duration => TimeSpan.FromSeconds(Seconds);
}
