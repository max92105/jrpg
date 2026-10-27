namespace Jrpg.Core.Effects;

/// <summary>What the target's resistance to a hit's type did to it (decision 323), for the screen to cue.</summary>
public enum HitAffinity
{
    Neutral,

    /// <summary>A weakness, a resistance below 0, raised the hit.</summary>
    Weak,

    /// <summary>A resistance of <see cref="DamageFormula.ResistedThresholdPercent"/> % or more lowered it.</summary>
    Resisted,
}
