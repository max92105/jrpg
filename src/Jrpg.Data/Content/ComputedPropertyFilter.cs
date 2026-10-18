using System.Text.Json.Serialization.Metadata;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves computed properties, such as <c>GridCell.Center</c>, out of content files. They can't be read back, so a
/// written value would only mislead someone editing the file. Done here so Core needs no serializer attributes (ADR 0002).
/// </summary>
internal static class ComputedPropertyFilter
{
    public static void RemoveComputedProperties(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        for (var index = typeInfo.Properties.Count - 1; index >= 0; index--)
        {
            var property = typeInfo.Properties[index];
            var isComputed = property.Set is null && property.AssociatedParameter is null;
            if (isComputed)
            {
                typeInfo.Properties.RemoveAt(index);
            }
        }
    }
}
