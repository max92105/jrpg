namespace Jrpg.Game.Battle;

/// <summary>
/// How long each part of a turn takes on screen: a step and its delays. Placeholders until a battle-speed option
/// exists.
/// </summary>
internal static class BattlePacing
{
    /// <summary>How far a party member steps out of the formation, in pixels.</summary>
    public const int StepDistance = 50;

    /// <summary>One step, forward or back.</summary>
    public static TimeSpan Step { get; } = TimeSpan.FromSeconds(0.2);

    /// <summary>From an action's start to its impact, as the reference clip times it (decision 644).</summary>
    public static TimeSpan WindUp { get; } = TimeSpan.FromSeconds(0.7);

    /// <summary>How far into the wind-up an Attack's swing starts: the clip's slash (decision 657).</summary>
    public static TimeSpan SwingStart { get; } = TimeSpan.FromSeconds(0.47);

    /// <summary>
    /// The hold after an action that lands on someone: shorter since the wind-up shows the action before it lands (the
    /// clip's number is gone by 0.57 s, its sparks by about 0.5 s).
    /// </summary>
    public static TimeSpan ImpactHold { get; } = TimeSpan.FromSeconds(0.85);

    /// <summary>The hold after an action that fells someone: long enough to see them go.</summary>
    public static TimeSpan DefeatHold { get; } = TimeSpan.FromSeconds(2.0);

    /// <summary>How long a lost turn holds its "Stunned" cue: long enough to read it.</summary>
    public static TimeSpan SkipHold { get; } = TimeSpan.FromSeconds(0.8);

    /// <summary>How long a backfire holds its cue and the HP it cost: long enough to read both.</summary>
    public static TimeSpan BackfireHold { get; } = TimeSpan.FromSeconds(1.0);

    /// <summary>How long the HP that statuses took or restored at a turn's end holds: long enough to read it.</summary>
    public static TimeSpan TickHold { get; } = TimeSpan.FromSeconds(0.8);

    /// <summary>How long a phase change holds its cue: long enough to read it (placeholder).</summary>
    public static TimeSpan PhaseHold { get; } = TimeSpan.FromSeconds(1.0);
}
