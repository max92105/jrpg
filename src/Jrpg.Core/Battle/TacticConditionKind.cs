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
}
