namespace Jrpg.Game.Input;

/// <summary>What the player asks for, whichever key or controller button they used (<see cref="InputBindings"/>).</summary>
internal enum GameButton
{
    Confirm,
    Cancel,
    Menu,
    Up,
    Down,
    Left,
    Right,
    PreviousTab,
    NextTab,

    /// <summary>Shows or hides extra detail, such as the inventory's item description (decision 291).</summary>
    Info,
}
