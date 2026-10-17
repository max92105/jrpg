namespace Jrpg.Core.Battle;

/// <summary>One enemy type. Every enemy of this type starts a battle at full HP and MP.</summary>
public sealed record EnemyDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string PortraitFile { get; init; }

    public required string BattleSpriteFile { get; init; }

    public required string AttackEffectFile { get; init; }

    public required bool IsSpriteFlipped { get; init; }

    public required int MaxHp { get; init; }

    public required int MaxMp { get; init; }

    public required int Attack { get; init; }

    public required int Defense { get; init; }

    public required int ExperienceReward { get; init; }
}
