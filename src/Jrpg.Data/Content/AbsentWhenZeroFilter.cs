using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Maps;

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
    ];

    public static void SkipZeroOptionalFields(JsonTypeInfo typeInfo)
    {
        foreach (var (type, field) in _addedLater)
        {
            if (typeInfo.Type == type)
            {
                typeInfo.Properties.Single(property => property.Name == field).ShouldSerialize = (_, value) => value is not 0;
            }
        }
    }
}
