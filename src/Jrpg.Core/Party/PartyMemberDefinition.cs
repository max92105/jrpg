namespace Jrpg.Core.Party;

/// <summary>Who a party member is; their numbers come from their class at their level (ADR 0017).</summary>
public sealed record PartyMemberDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string ClassId { get; init; }

    /// <summary>Who the character is, shown when choosing a leader (decision 167).</summary>
    public required string Summary { get; init; }

    public required string PortraitFile { get; init; }

    public required string BattleSpriteFile { get; init; }

    public required string AttackEffectFile { get; init; }
}
