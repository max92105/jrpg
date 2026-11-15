using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>A zoom over the virtual screen (decision 752): its scale, and the screen point that stays put.</summary>
/// <param name="Scale">1 is no zoom; above 1 grows the picture about <paramref name="Origin"/>.</param>
/// <param name="Origin">The point of the screen, in virtual pixels, that doesn't move.</param>
internal readonly record struct ScreenZoom(float Scale, Vector2 Origin)
{
    /// <summary>Where a point of the unzoomed screen shows under the zoom.</summary>
    public Vector2 PositionOf(Vector2 point) => Origin + ((point - Origin) * Scale);
}
