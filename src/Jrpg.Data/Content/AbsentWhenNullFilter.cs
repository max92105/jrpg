using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;

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
