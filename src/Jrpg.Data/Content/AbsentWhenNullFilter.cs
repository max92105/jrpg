using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves an optional field added after the content was written out of a file while it is null, so every file that
/// predates it writes back byte for byte. Elsewhere a null is written, as the files have it.
/// </summary>
internal static class AbsentWhenNullFilter
{
    private static readonly (Type Type, string Field)[] _addedLater =
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

        // ADR 0039: only animated tiles have an animation.
        (typeof(TileDefinition), "animation"),
    ];

    public static void SkipNullOptionalFields(JsonTypeInfo typeInfo)
    {
        foreach (var (type, field) in _addedLater)
        {
            if (typeInfo.Type == type)
            {
                typeInfo.Properties.Single(property => property.Name == field).ShouldSerialize = (_, value) => value is not null;
            }
        }
    }
}
