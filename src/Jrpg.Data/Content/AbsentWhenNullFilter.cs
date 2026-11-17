using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Battle;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Effects;
using Jrpg.Core.Events;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Core.Skills;
using Jrpg.Core.Sprites;
using Jrpg.Core.Statuses;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves an optional field out of a file while it is null, for two reasons: a field added after the content was
/// written, so every file that predates it writes back byte for byte; and a field with a meaning for null (an enemy's,
/// decision 586; a cinematic's, ADR 0046), so a record the editor writes reads like a hand-written one. Every other
/// null is written out, as the hand-written files spell it.
/// </summary>
internal static class AbsentWhenNullFilter
{
    private static readonly (Type Type, string Field)[] _absentByDefault =
    [
        // ADR 0015: only NPCs have a field sprite.
        (typeof(ObjectDefinition), "fieldSpriteFile"),

        // Decision 185: only the intros are gated on the leader.
        (typeof(MapEvent), "onlyWhenLeaderId"),

        // ADR 0033: only quest-gated events have a condition.
        (typeof(MapEvent), "onlyWhenQuest"),

        // ADR 0033: only the two stage kinds name a stage.
        (typeof(QuestCondition), "stageId"),

        // Decision 382: only an inn prices a rest.
        (typeof(RestStep), "price"),

        // Decision 419: a map without a track is silent.
        (typeof(MapSettings), "musicFile"),

        // Decision 527: a town has no area level.
        (typeof(MapSettings), "areaLevel"),

        // Decision 532: a map at the top of the editor's list has no folder.
        (typeof(MapSettings), "folder"),

        // Decision 892: a map without random battles may name no battle background.
        (typeof(MapSettings), "battleBackgroundFile"),

        // ADR 0039: only animated tiles have an animation.
        (typeof(TileDefinition), "animation"),

        // ADR 0044: only a turned NPC has a facing.
        (typeof(PlacedObject), "facing"),

        // Decision 800: a status without tiers is its own family.
        (typeof(StatusDefinition), "family"),

        // Decision 810: a mark shows its name's first letter unless the record names another.
        (typeof(StatusDefinition), "markLetter"),

        // Decision 710: a skill without its own strip plays its user's.
        (typeof(SkillDefinition), "effectFile"),

        // A consumable has an effect, gear its equipment, a material neither (decision 339).
        (typeof(ItemDefinition), "effect"),
        (typeof(ItemDefinition), "equipment"),

        // ADR 0031: an item nobody trades has no price.
        (typeof(ItemDefinition), "price"),

        // Decision 678: an item without its own strip plays its user's.
        (typeof(ItemDefinition), "effectFile"),

        // ADR 0027: only a weapon has a type.
        (typeof(EquipmentStats), "damageType"),

        // ADR 0027: only a damage effect has a type.
        (typeof(Effect), "damageType"),

        // ADR 0056: a flat power names no tier.
        (typeof(Effect), "tier"),

        // ADR 0056: a heal record has no type.
        (typeof(TieredEffect), "damageType"),

        // Decision 803: only a typed tick names its type.
        (typeof(StatusPart), "damageType"),

        // Decision 807: only a stat change names its stat.
        (typeof(StatusPart), "stat"),

        // ADR 0054: a walk whose facings all read their rows has no sequences, and a facing that reads its row has no
        // sequence of its own.
        (typeof(FacingCycle), "sequences"),
        (typeof(FacingSequences), "down"),
        (typeof(FacingSequences), "up"),
        (typeof(FacingSequences), "left"),
        (typeof(FacingSequences), "right"),
        (typeof(FacingSequence), "mirrorOf"),

        // Decision 586: an enemy the editor writes reads like a hand-written one; null drops nothing, no gold.
        (typeof(EnemyDefinition), "dropTableId"),
        (typeof(EnemyDefinition), "gold"),

        // ADR 0047: only an animated sprite has a loop.
        (typeof(EnemyDefinition), "idleAnimation"),

        // Decision 908: an encounter without its own background fights on the map's, one without its own track plays
        // the battle theme.
        (typeof(EncounterDefinition), "backgroundFile"),
        (typeof(EncounterDefinition), "musicFile"),

        // ADR 0048: only a strip an Attack sweeps before names a swing.
        (typeof(EffectStripDefinition), "swingFile"),

        // ADR 0046: a cinematic without a track keeps the map's; a still may have no subtitle, voice, zoom or focus.
        (typeof(CinematicDefinition), "musicFile"),
        (typeof(CinematicStill), "caption"),
        (typeof(CinematicStill), "voiceFile"),
        (typeof(CinematicStill), "zoom"),
        (typeof(CinematicStill), "focus"),

        // Decision 412: a null skill is the basic attack; a null condition is always.
        (typeof(EnemyAction), "skillId"),
        (typeof(EnemyAction), "when"),

        // Decision 848: most rules aim as their skill does.
        (typeof(EnemyAction), "on"),
    ];

    public static void SkipNullOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(typeInfo, _absentByDefault, value => value is null);
}
