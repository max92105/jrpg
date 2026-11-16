namespace Jrpg.Core.Battle;

/// <summary>The closed list of conditions an enemy's action may carry (ADR 0034).</summary>
public enum TacticConditionKind
{
    /// <summary>The enemy's own HP is under the share.</summary>
    OwnHpBelow,

    /// <summary>A living ally's HP is under the share.</summary>
    AllyHpBelow,

    /// <summary>An ally has fallen.</summary>
    AllyFallen,

    /// <summary>A damage hit landed on the enemy since its last turn, by a foe's action (decision 845).</summary>
    WasHit,

    /// <summary>A damage hit crit the enemy since its last turn, by a foe's action.</summary>
    WasCriticallyHit,

    /// <summary>A damage hit landed on the enemy's weakness since its last turn, by a foe's action.</summary>
    WasHitOnWeakness,
}
