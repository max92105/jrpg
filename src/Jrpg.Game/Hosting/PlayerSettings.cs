namespace Jrpg.Game.Hosting;

/// <summary>
/// The options the player chose, for this run only (question 89). One object owned by <c>Program</c>: the host applies the
/// window mode when it changes, the Options page edits it, and dialogue reads the text speed.
/// </summary>
internal sealed class PlayerSettings
{
    /// <summary>Borderless full screen, the game's normal mode (decision 78); false for the resizable window.</summary>
    public bool IsFullScreen { get; set; } = true;

    /// <summary>Normal is decision 54's 30 ms per character.</summary>
    public TextSpeed TextSpeed { get; set; } = TextSpeed.Normal;

    /// <summary>The time each dialogue character takes to appear; zero shows a page whole.</summary>
    public TimeSpan CharacterTime => TextSpeed switch
    {
        TextSpeed.Slow => TimeSpan.FromMilliseconds(45),
        TextSpeed.Normal => TimeSpan.FromMilliseconds(30),
        TextSpeed.Fast => TimeSpan.FromMilliseconds(15),
        TextSpeed.Instant => TimeSpan.Zero,
        _ => throw new InvalidOperationException($"Text speed {TextSpeed} has no character time."),
    };
}
