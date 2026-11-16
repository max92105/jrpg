namespace Jrpg.Core.Battle;

/// <summary>One weighted choice in an enemy's tactics (ADR 0034).</summary>
public sealed record EnemyAction
{
    /// <summary>The skill it uses, by id; null is the basic attack (decision 412).</summary>
    public string? SkillId { get; init; }

    /// <summary>Its share of the pick: this weight over the eligible actions' total. At least 1.</summary>
    public required int Weight { get; init; }

    /// <summary>When it may be chosen (decision 402); null is always.</summary>
    public TacticCondition? When { get; init; }

    /// <summary>
    /// Whom it aims at (decision 834); null aims as the skill or the attack always did (decisions 400, 401).
    /// </summary>
    public TacticTarget? On { get; init; }

    /// <summary>
    /// Chosen before any draw while eligible, the first in the list among several (decision 841).
    /// </summary>
    public bool IsPriority { get; init; }

    /// <summary>
    /// Not eligible once the enemy used its skill this battle, through any rule (decision 842); never on the attack.
    /// </summary>
    public bool IsOncePerBattle { get; init; }
}
