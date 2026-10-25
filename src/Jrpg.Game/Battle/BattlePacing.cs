namespace Jrpg.Game.Battle;

/// <summary>
/// How long each part of a turn takes on screen: a step and its delays. Placeholders until a battle-speed option
/// exists.
/// </summary>
internal static class BattlePacing
{
    /// <summary>How far an actor steps out of the formation, in pixels.</summary>
    public const int StepDistance = 50;

    /// <summary>One step, forward or back.</summary>
    public static TimeSpan Step { get; } = TimeSpan.FromSeconds(0.2);

    /// <summary>The hold after an action that lands on someone.</summary>
    public static TimeSpan ImpactHold { get; } = TimeSpan.FromSeconds(1.0);

    /// <summary>The hold after an action that fells someone: long enough to see them go.</summary>
    public static TimeSpan DefeatHold { get; } = TimeSpan.FromSeconds(2.0);

    /// <summary>How long a lost turn holds its "Stunned" cue: long enough to read it.</summary>
    public static TimeSpan SkipHold { get; } = TimeSpan.FromSeconds(0.8);

    /// <summary>How long a backfire holds its cue and the HP it cost: long enough to read both.</summary>
    public static TimeSpan BackfireHold { get; } = TimeSpan.FromSeconds(1.0);
}
