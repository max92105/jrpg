using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Content;

/// <summary>All loaded content definitions, by id, and the global rules.</summary>
public sealed record ContentCatalog
{
    public required IReadOnlyDictionary<string, ItemDefinition> Items { get; init; }

    public required IReadOnlyDictionary<string, ClassDefinition> Classes { get; init; }

    public required IReadOnlyDictionary<string, SkillDefinition> Skills { get; init; }

    public required IReadOnlyDictionary<string, PartyMemberDefinition> PartyMembers { get; init; }

    public required IReadOnlyDictionary<string, EnemyDefinition> Enemies { get; init; }

    public required IReadOnlyDictionary<string, EncounterDefinition> Encounters { get; init; }

    public required IReadOnlyDictionary<int, TileDefinition> Tiles { get; init; }

    public required IReadOnlyDictionary<string, TilesheetDefinition> Tilesheets { get; init; }

    public required IReadOnlyDictionary<string, ObjectDefinition> Objects { get; init; }

    public required IReadOnlyDictionary<string, MapDefinition> Maps { get; init; }

    public required GameRules Rules { get; init; }

    public required NewGameDefinition NewGame { get; init; }

    /// <summary>The affixes dropped gear may roll, by id; a saved affix's stat is read from here (ADR 0029).</summary>
    public required IReadOnlyDictionary<string, AffixDefinition> Affixes { get; init; }

    /// <summary>The class that gives a member their numbers at their level (ADR 0017).</summary>
    public ClassDefinition ClassOf(PartyMemberDefinition member) => Classes[member.ClassId];

    /// <summary>A saved piece back as a piece, each affix's stat read from the pool.</summary>
    /// <remarks>For a repaired save, as <c>GameSession.Restore</c> takes: its base and affixes are known.</remarks>
    public GearPiece PieceFrom(SavedGear saved) =>
        GearPiece.Rolled(Items[saved.ItemId], saved.Rarity, [.. saved.Affixes.Select(affix => new RolledAffix(affix.AffixId, Affixes[affix.AffixId].Stat, affix.Value))]);
}
