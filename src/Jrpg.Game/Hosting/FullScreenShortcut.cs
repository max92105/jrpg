using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game.Hosting;

/// <summary>Alt+Enter, the usual Windows shortcut between full screen and a window.</summary>
internal static class FullScreenShortcut
{
    /// <summary>Whether Enter went down this update while either Alt key is held.</summary>
    /// <param name="previous">The keyboard on the previous update.</param>
    /// <param name="current">The keyboard on this update.</param>
    public static bool IsPressed(KeyboardState previous, KeyboardState current) =>
        current.IsKeyDown(Keys.Enter) && previous.IsKeyUp(Keys.Enter) && IsAltHeld(current);

    private static bool IsAltHeld(KeyboardState keyboard) => keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
}
