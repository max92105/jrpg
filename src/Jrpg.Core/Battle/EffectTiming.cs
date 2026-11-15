namespace Jrpg.Core.Battle;

/// <summary>
/// How fast a battle effect strip plays (decision 57), in one place for the game that plays it and the editor that
/// reports an imported effect's length.
/// </summary>
public static class EffectTiming
{
    /// <summary>Each square frame of an effect strip shows this long.</summary>
    public static TimeSpan FrameTime { get; } = TimeSpan.FromSeconds(0.08);
}
