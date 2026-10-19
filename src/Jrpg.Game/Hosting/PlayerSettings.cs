namespace Jrpg.Game.Hosting;

/// <summary>
/// The options the player chose, for this run only (question 89). One object owned by <c>Program</c>: the host applies the
/// window mode when it changes.
/// </summary>
internal sealed class PlayerSettings
{
    /// <summary>Borderless full screen, the game's normal mode (decision 78); false for the resizable window.</summary>
    public bool IsFullScreen { get; set; } = true;
}
