using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Battle;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;

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

        // ADR 0039: only animated tiles have an animation.
        (typeof(TileDefinition), "animation"),

        // ADR 0044: only a turned NPC has a facing.
        (typeof(PlacedObject), "facing"),

        // Decision 586: an enemy the editor writes reads like a hand-written one; null drops nothing, no gold.
        (typeof(EnemyDefinition), "dropTableId"),
        (typeof(EnemyDefinition), "gold"),

        // ADR 0046: a cinematic without a track keeps the map's; a still may have no subtitle, voice or zoom.
        (typeof(CinematicDefinition), "musicFile"),
        (typeof(CinematicStill), "caption"),
        (typeof(CinematicStill), "voiceFile"),
        (typeof(CinematicStill), "zoom"),

        // Decision 412: a null skill is the basic attack; a null condition is always.
        (typeof(EnemyAction), "skillId"),
        (typeof(EnemyAction), "when"),
    ];

    public static void SkipNullOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(typeInfo, _absentByDefault, value => value is null);
}
