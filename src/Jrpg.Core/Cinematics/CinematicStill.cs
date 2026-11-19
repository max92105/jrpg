using Jrpg.Core.Geometry;

namespace Jrpg.Core.Cinematics;

/// <summary>
/// One still of a cinematic as content: its picture, how long it shows, its subtitles in sequence, voice line, zoom and
/// the point the zoom heads to.
/// </summary>
public sealed record CinematicStill
{
    /// <summary>
    /// The picture under the cinematics art folder, its cinematic's folder included
    /// (<c>ragnir_intro/1_happy_life.png</c>, decision 610).
    /// </summary>
    public required string ImageFile { get; init; }

    /// <summary>How long the still shows, fades included; above 0.</summary>
    public required double Seconds { get; init; }

    /// <summary>
    /// The subtitles shown one after the other from the still's start, each for its own time; none shows once the last
    /// one's time is over. Empty for a still without subtitles.
    /// </summary>
    public IReadOnlyList<StillCaption> Captions { get; init; } = [];

    /// <summary>A voice line in the voices folder, started with the still; null starts none (decision 613).</summary>
    public string? VoiceFile { get; init; }

    /// <summary>The slow zoom over the still's time; null draws it at its fitted size.</summary>
    public StillZoom? Zoom { get; init; }

    /// <summary>
    /// Where the zoom heads, null for the center (decision 746). Kept without a zoom, where it changes nothing
    /// (decision 750), and kept when the picture changes: it is a share of whatever picture shows (decision 747).
    /// </summary>
    public StillFocus? Focus { get; init; }

    public TimeSpan Duration => TimeSpan.FromSeconds(Seconds);

    /// <summary>
    /// The share of the shown picture that stays put while the still zooms: its focus, else the center; read by the
    /// game and the editor's preview (decision 751).
    /// </summary>
    public Point2D ZoomOrigin => (Focus ?? StillFocus.Center).Share;

    /// <summary>
    /// How large the still draws at a time into it, as a factor of its fitted size (decision 617): 1 without a zoom;
    /// held at the zoom's end from the still's end on.
    /// </summary>
    public double ScaleAt(TimeSpan intoStill) =>
        Zoom is { } zoom ? zoom.ScaleAt(Math.Clamp(intoStill / Duration, 0, 1)) : 1;

    /// <summary>
    /// The subtitle shown at a time into the still: the one whose turn it is in the sequence; null before the still
    /// starts, during a pause, and once the last one's time is over.
    /// </summary>
    public StillCaption? CaptionAt(TimeSpan intoStill)
    {
        var start = TimeSpan.Zero;
        foreach (var caption in Captions)
        {
            var end = start + caption.Duration;
            if (intoStill >= start && intoStill < end)
            {
                return caption.Text.Length > 0 ? caption : null;
            }

            start = end;
        }

        return null;
    }

    // The generated Equals compares the subtitles by reference; a changed-record check needs them by content.
    public bool Equals(CinematicStill? other) =>
        other is not null
        && ImageFile == other.ImageFile
        && Seconds.Equals(other.Seconds)
        && VoiceFile == other.VoiceFile
        && Zoom == other.Zoom
        && Focus == other.Focus
        && Captions.SequenceEqual(other.Captions);

    public override int GetHashCode() => HashCode.Combine(ImageFile, Seconds, VoiceFile, Zoom, Focus, Captions.Count);
}
