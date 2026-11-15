using Jrpg.Core.Geometry;

namespace Jrpg.Core.Cinematics;

/// <summary>
/// One still of a cinematic as content: its picture, how long it shows, its subtitle, voice line, zoom and the point
/// the zoom heads to.
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

    /// <summary>The subtitle shown under the still; null shows none.</summary>
    public string? Caption { get; init; }

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
}
