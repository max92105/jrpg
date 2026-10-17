namespace Jrpg.Core.Party;

/// <summary>Who a party member is and the values they join with.</summary>
public sealed record PartyMemberDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string ClassId { get; init; }

    public required string PortraitFile { get; init; }

    public required string BattleSpriteFile { get; init; }

    public required string AttackEffectFile { get; init; }

    public required int Level { get; init; }

    /// <summary>Experience earned toward the next level, not the lifetime total.</summary>
    public required int Experience { get; init; }

    public required int Hp { get; init; }

    public required int MaxHp { get; init; }

    public required int Mp { get; init; }

    public required int MaxMp { get; init; }

    public required int Attack { get; init; }

    public required int Defense { get; init; }

    /// <summary>Ids of the skills the member joins with, resolved against the skill catalog.</summary>
    public required IReadOnlyList<string> SkillIds { get; init; }
}
