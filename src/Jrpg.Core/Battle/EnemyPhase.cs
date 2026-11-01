namespace Jrpg.Core.Battle;

/// <summary>The actions an enemy switches to below a share of its HP (decision 404); it changes nothing else.</summary>
public sealed record EnemyPhase
{
    /// <summary>A whole percent of max HP, 1..99: entered once HP is strictly under this share.</summary>
    public required int BelowHpPercent { get; init; }

    public required IReadOnlyList<EnemyAction> Tactics { get; init; }
}
