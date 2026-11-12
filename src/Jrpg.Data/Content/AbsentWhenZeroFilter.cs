using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a whole-number field added after the content was written out of a file while it is 0, so every file that
/// predates it writes back byte for byte.
/// </summary>
internal static class AbsentWhenZeroFilter
{
    private static readonly (Type Type, string Field)[] _addedLater =
    [
        // ADR 0040: only stacked tiles have a level above the base.
        (typeof(ObjectTile), "level"),
        // ADR 0043: most sheets stand on the frame's bottom edge.
        (typeof(FieldSpriteSheet), "feetPixels"),
    ];

    public static void SkipZeroOptionalFields(JsonTypeInfo typeInfo) => OptionalFieldFilter.SkipWhen(typeInfo, _addedLater, value => value is 0);
}
