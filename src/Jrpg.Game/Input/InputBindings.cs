using System.Collections.Immutable;
using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game.Input;

/// <summary>
/// Which keys and controller buttons press which game button: the legacy keys (arrows, E/Space/Enter, Esc, F1), WASD and
/// Backspace (developer decision 50), and a standard controller layout. Legacy menus were mouse-driven, so the tab
/// buttons have no key yet (question 92).
/// </summary>
internal static class InputBindings
{
    public static ImmutableArray<(Keys Key, GameButton Button)> KeyBindings { get; } =
    [
        (Keys.Up, GameButton.Up),
        (Keys.Down, GameButton.Down),
        (Keys.Left, GameButton.Left),
        (Keys.Right, GameButton.Right),
        (Keys.W, GameButton.Up),
        (Keys.S, GameButton.Down),
        (Keys.A, GameButton.Left),
        (Keys.D, GameButton.Right),
        (Keys.E, GameButton.Confirm),
        (Keys.Space, GameButton.Confirm),
        (Keys.Enter, GameButton.Confirm),

        // Legacy Esc both opened the menu on the map and closed whatever was open.
        (Keys.Escape, GameButton.Cancel),
        (Keys.Escape, GameButton.Menu),
        (Keys.Back, GameButton.Cancel),
        (Keys.F1, GameButton.DebugCutscene),
    ];

    public static ImmutableArray<(Buttons PadButton, GameButton Button)> PadBindings { get; } =
    [
        (Buttons.DPadUp, GameButton.Up),
        (Buttons.DPadDown, GameButton.Down),
        (Buttons.DPadLeft, GameButton.Left),
        (Buttons.DPadRight, GameButton.Right),
        (Buttons.A, GameButton.Confirm),
        (Buttons.B, GameButton.Cancel),
        (Buttons.Start, GameButton.Menu),
        (Buttons.LeftShoulder, GameButton.PreviousTab),
        (Buttons.RightShoulder, GameButton.NextTab),
    ];
}
