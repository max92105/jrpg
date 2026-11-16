namespace Jrpg.Core.Battle;

/// <summary>
/// The closed list of whom an enemy's rule aims at (decision 834); null on an action is automatic, aiming as the skill
/// or the attack always did (decisions 400, 401).
/// </summary>
public enum TacticTarget
{
    /// <summary>
    /// Whom its condition names ("that one"), the lowest HP share among them still legal (decision 839).
    /// </summary>
    Subject,

    /// <summary>The enemy itself.</summary>
    Self,

    /// <summary>The foe with the lowest HP share.</summary>
    LowestHpFoe,

    /// <summary>The foe with the highest HP share.</summary>
    HighestHpFoe,

    /// <summary>The lowest HP share on its side, itself included.</summary>
    LowestHpOnItsSide,

    /// <summary>
    /// A random foe holding no family of the skill's harmful statuses, so a poisoner spreads its poison instead of
    /// renewing it on the same foe.
    /// </summary>
    UnafflictedFoe,
}
