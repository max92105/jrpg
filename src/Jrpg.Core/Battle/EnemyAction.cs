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
}
