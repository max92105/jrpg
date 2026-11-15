using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Sprites;

namespace Jrpg.Data.Content;

/// <summary>
/// Marks members of content structs required, as <c>required</c> marks a class's (decision 768): the serializer builds
/// a struct through its parameterless constructor, so a missing member would read as 0 instead of being refused.
/// <c>ObjectTile</c> and <c>GridCell</c> have the same gap and could join the list.
/// </summary>
internal static class RequiredStructMembers
{
    private static readonly (Type Type, string Member)[] _required =
    [
        // ADR 0054: a sequence's cell names both its column and its row.
        (typeof(SpriteCell), "column"),
        (typeof(SpriteCell), "row"),
    ];

    public static void MarkRequired(JsonTypeInfo typeInfo)
    {
        foreach (var property in typeInfo.Properties)
        {
            if (Array.Exists(_required, required => required.Type == typeInfo.Type && required.Member == property.Name))
            {
                property.IsRequired = true;
            }
        }
    }
}
