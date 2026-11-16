namespace Jrpg.Core.Battle;

/// <summary>The number a trigger kind reads from its condition, which content sets only for that kind.</summary>
public enum TacticConditionNumber
{
    /// <summary>It reads none, so content leaves the number out.</summary>
    None,

    /// <summary>A whole percent of max HP, 1..99.</summary>
    Percent,

    /// <summary>A count of the enemy's own turns acted, 2..10.</summary>
    Turns,
}
