using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Maps;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves an optional field added after the content was written (today only <c>fieldSpriteFile</c> on objects) out of a
/// file while it is null, so every file that predates it writes back byte for byte. Elsewhere a null is written, as the
/// files have it.
/// </summary>
internal static class AbsentWhenNullFilter
{
    public static void SkipNullOptionalFields(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(ObjectDefinition))
        {
            return;
        }

        // ADR 0015: only NPCs have a field sprite.
        var fieldSprite = typeInfo.Properties.Single(property => property.Name == "fieldSpriteFile");
        fieldSprite.ShouldSerialize = (_, value) => value is not null;
    }
}
