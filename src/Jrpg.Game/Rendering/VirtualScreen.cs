using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Rendering;

/// <summary>
/// The game draws at one fixed size and is fitted into whatever window it gets (developer decision 48): the aspect
/// ratio is kept and the rest of the window is left black. The fit is a whole-number scale exactly when the window is
/// an exact multiple of the virtual size (1920 x 1080 at 1x, 3840 x 2160 at 2x), and a fractional one otherwise.
/// </summary>
internal static class VirtualScreen
{
    public const int Width = 1920;
    public const int Height = 1080;

    // MonoGame's BlendState.Additive multiplies the source by its alpha again, which squares a premultiplied color's
    // fade; light added as it is fades as its color does. Never changed once made, and the device takes it as is.
    private static readonly BlendState _addedLight = new()
    {
        ColorSourceBlend = Blend.One,
        AlphaSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
    };

    /// <summary>
    /// Begins the batch that draws the scenes onto the virtual screen: point-sampled, so pixel art stays crisp. A view
    /// that ends it midway, as the shatter does, begins it again with this.
    /// </summary>
    public static void BeginBatch(SpriteBatch batch) => batch.Begin(samplerState: SamplerState.PointClamp);

    /// <summary>
    /// Begins a batch that adds light over what is drawn (ADR 0049): premultiplied colors added as they are, sampled
    /// linearly, as soft light is never pixel art. The caller ends it, then begins <see cref="BeginBatch"/> again.
    /// </summary>
    public static void BeginLightBatch(SpriteBatch batch) =>
        batch.Begin(blendState: _addedLight, samplerState: SamplerState.LinearClamp);

    /// <summary>Where the virtual screen lands in the window, centered.</summary>
    public static Rectangle DestinationIn(Rectangle window)
    {
        var scale = Math.Min((double)window.Width / Width, (double)window.Height / Height);
        var width = (int)Math.Round(Width * scale);
        var height = (int)Math.Round(Height * scale);
        return new Rectangle(window.X + ((window.Width - width) / 2), window.Y + ((window.Height - height) / 2), width, height);
    }
}
