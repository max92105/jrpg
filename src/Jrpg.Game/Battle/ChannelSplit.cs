using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws a sprite as three single-channel copies (decision 661, ADR 0049): red shifted left, green shifted right, blue
/// in place. Each copy blends as the sprite would, into its one color channel only, so where nothing moves the three
/// rebuild the sprite exactly and the split ends without a jump; at its edges they fringe red on one side and green on
/// the other. Called inside the scenes' batch (<see cref="VirtualScreen.BeginBatch"/>), which it ends and begins again.
/// </summary>
internal static class ChannelSplit
{
    // BlendState.AlphaBlend's premultiplied blend, each writing one channel. Never changed once made.
    private static readonly BlendState _redOnly = AlphaBlendInto(ColorWriteChannels.Red);
    private static readonly BlendState _greenOnly = AlphaBlendInto(ColorWriteChannels.Green);
    private static readonly BlendState _blueOnly = AlphaBlendInto(ColorWriteChannels.Blue);

    /// <param name="batch">The scenes' batch, begun.</param>
    /// <param name="sprite">The sprite as it would be drawn once.</param>
    /// <param name="pixels">
    /// Virtual px each of red and green moves away from blue (<see cref="ColorFringe.PixelsAt"/>).
    /// </param>
    /// <param name="strength">The sprite's opacity, from 0 to 1.</param>
    public static void Draw(SpriteBatch batch, DrawnSprite sprite, int pixels, float strength)
    {
        DrawCopy(batch, sprite, (-pixels, _redOnly), strength);
        DrawCopy(batch, sprite, (pixels, _greenOnly), strength);
        DrawCopy(batch, sprite, (0, _blueOnly), strength);
        batch.End();
        VirtualScreen.BeginBatch(batch);
    }

    private static void DrawCopy(SpriteBatch batch, DrawnSprite sprite, (int Offset, BlendState Channel) copy, float strength)
    {
        batch.End();
        batch.Begin(blendState: copy.Channel, samplerState: SamplerState.PointClamp);
        sprite.Draw(batch, new Point(copy.Offset, 0), Color.White * strength);
    }

    private static BlendState AlphaBlendInto(ColorWriteChannels channel) => new()
    {
        ColorSourceBlend = Blend.One,
        AlphaSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.InverseSourceAlpha,
        AlphaDestinationBlend = Blend.InverseSourceAlpha,
        ColorWriteChannels = channel,
    };
}
