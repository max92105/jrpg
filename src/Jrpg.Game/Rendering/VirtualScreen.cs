using Microsoft.Xna.Framework;

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

    /// <summary>Where the virtual screen lands in the window, centered.</summary>
    public static Rectangle DestinationIn(Rectangle window)
    {
        var scale = Math.Min((double)window.Width / Width, (double)window.Height / Height);
        var width = (int)Math.Round(Width * scale);
        var height = (int)Math.Round(Height * scale);
        return new Rectangle(window.X + ((window.Width - width) / 2), window.Y + ((window.Height - height) / 2), width, height);
    }
}
