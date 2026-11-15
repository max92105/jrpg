using Jrpg.Core.Cinematics;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>A still as it shows this frame: its picture, how faded and how zoomed (decisions 617, 629).</summary>
/// <param name="ImageFile">The picture under the cinematics folder.</param>
/// <param name="Opacity">0 to 1: the picture and its subtitle fade with it.</param>
/// <param name="Zoom">
/// The picture's size as a factor of its fitted size, about the still's focus on the screen, the center without one.
/// </param>
internal readonly record struct StillFrame(string ImageFile, float Opacity, ScreenZoom Zoom)
{
    /// <summary>The still at a time into it, from Core's fade, scale and zoom origin (decision 751).</summary>
    public static StillFrame At(CinematicStill still, TimeSpan intoStill)
    {
        var origin = still.ZoomOrigin;
        var zoom = new ScreenZoom(
            (float)still.ScaleAt(intoStill),
            new Vector2((float)(origin.X * VirtualScreen.Width), (float)(origin.Y * VirtualScreen.Height)));
        return new StillFrame(still.ImageFile, StillFade.OpacityAt(intoStill, still.Duration), zoom);
    }
}
