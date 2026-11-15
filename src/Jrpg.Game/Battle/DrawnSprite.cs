using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// A texture's part where it is drawn: one draw, shared by a sprite drawn once and by its split copies, so the copies
/// line up with it.
/// </summary>
/// <param name="Texture">The image.</param>
/// <param name="Destination">Where on the virtual screen.</param>
/// <param name="Source">Its part of the image; null for the whole.</param>
/// <param name="Effects">Its flip.</param>
internal readonly record struct DrawnSprite(Texture2D Texture, Rectangle Destination, Rectangle? Source, SpriteEffects Effects)
{
    /// <param name="batch">A begun batch.</param>
    /// <param name="offset">Virtual px from <see cref="Destination"/>.</param>
    /// <param name="tint">Premultiplied, as every texture is.</param>
    public void Draw(SpriteBatch batch, Point offset, Color tint)
    {
        var destination = Destination;
        destination.Offset(offset);
        batch.Draw(Texture, destination, Source, tint, 0, Vector2.Zero, Effects, 0);
    }
}
