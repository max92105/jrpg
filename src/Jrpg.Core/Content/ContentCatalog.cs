using Jrpg.Core.Battle;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Quests;
using Jrpg.Core.Skills;
using Jrpg.Core.Sprites;
using Jrpg.Core.Statuses;
using Jrpg.Core.Trade;

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

    /// <summary>What defeated enemies drop, by table id (ADR 0030).</summary>
    public required IReadOnlyDictionary<string, DropTable> DropTables { get; init; }

    /// <summary>The shops a shopkeeper may open, by id (ADR 0031).</summary>
    public required IReadOnlyDictionary<string, ShopDefinition> Shops { get; init; }

    /// <summary>The quests the steps and conditions name, by id (ADR 0032).</summary>
    public required IReadOnlyDictionary<string, QuestDefinition> Quests { get; init; }

    /// <summary>
    /// Every field sprite image's layout by file name, exact case (ADR 0043); empty until content lists them.
    /// </summary>
    public IReadOnlyDictionary<string, FieldSpriteSheet> FieldSprites { get; init; } =
        new Dictionary<string, FieldSpriteSheet>(StringComparer.Ordinal);

    /// <summary>The statuses an effect grants, by id (ADR 0055).</summary>
    public IReadOnlyDictionary<string, StatusDefinition> Statuses { get; init; } = new Dictionary<string, StatusDefinition>();

    /// <summary>The tiered effects a skill's damage or heal names, by id (ADR 0056).</summary>
    public IReadOnlyDictionary<string, TieredEffect> TieredEffects { get; init; } = new Dictionary<string, TieredEffect>();

    /// <summary>The effect strips' paces, hit frames and swings by file name, exact case (ADR 0048).</summary>
    public IReadOnlyDictionary<string, EffectStripDefinition> EffectStrips { get; init; } =
        new Dictionary<string, EffectStripDefinition>(StringComparer.Ordinal);

    /// <summary>The cinematics an event's <c>cinematic</c> step plays, by id (ADR 0046).</summary>
    public IReadOnlyDictionary<string, CinematicDefinition> Cinematics { get; init; } =
        new Dictionary<string, CinematicDefinition>(StringComparer.Ordinal);

    /// <summary>The class that gives a member their numbers at their level (ADR 0017).</summary>
    public ClassDefinition ClassOf(PartyMemberDefinition member) => Classes[member.ClassId];

    /// <summary>
    /// A saved piece back as a piece, each affix's stat read from the pool; a unique from its record.
    /// </summary>
    /// <remarks>
    /// For a repaired save, as <c>GameSession.Restore</c> takes: its base and affixes are known. A unique's saved
    /// rarity and affixes are ignored (decision 868): its record is the truth, so an edited unique reaches every copy
    /// the party owns.
    /// </remarks>
    public GearPiece PieceFrom(SavedGear saved)
    {
        var item = Items[saved.ItemId];
        if (item.IsUnique)
        {
            return GearPiece.Unique(item);
        }

        var affixes = saved.Affixes.Select(affix => new RolledAffix(affix.AffixId, Affixes[affix.AffixId].Stat, affix.Value));
        return GearPiece.Rolled(item, saved.Rarity, [.. affixes]);
    }
}
