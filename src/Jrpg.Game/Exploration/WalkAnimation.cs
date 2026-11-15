namespace Jrpg.Game.Exploration;

/// <summary>
/// How long the player has walked since last standing, which the player's sheet turns into a walk frame
/// (<c>FieldSpriteSheet.WalkFrameAt</c>, ADR 0043); the record holds the frame count and time, not this.
/// </summary>
internal sealed class WalkAnimation
{
    /// <summary>The time walked since the last <see cref="Stand"/>; zero while standing.</summary>
    public TimeSpan Walked { get; private set; }

    /// <summary>
    /// Adds an update's delta: the time accumulates, so 60 Hz updates keep the record's exact frame rate.
    /// </summary>
    public void Advance(TimeSpan delta) => Walked += delta;

    public void Stand() => Walked = TimeSpan.Zero;
}
