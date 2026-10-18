using System.Collections.Immutable;
using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game.Input;

/// <summary>
/// Which keys and controller buttons press which game button. Placeholders until the developer settles the controls
/// (question 50): the legacy keys (arrows, E/Space/Enter, Esc, F1) and a standard controller layout. Legacy menus were
/// mouse-driven, so the tab buttons have no key yet.
/// </summary>
internal static class InputBindings
{
    public static ImmutableArray<(Keys Key, GameButton Button)> KeyBindings { get; } =
    [
        (Keys.Up, GameButton.Up),
        (Keys.Down, GameButton.Down),
        (Keys.Left, GameButton.Left),
        (Keys.Right, GameButton.Right),
        (Keys.E, GameButton.Confirm),
        (Keys.Space, GameButton.Confirm),
        (Keys.Enter, GameButton.Confirm),

        // Legacy Esc both opened the menu on the map and closed whatever was open.
        (Keys.Escape, GameButton.Cancel),
        (Keys.Escape, GameButton.Menu),
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
