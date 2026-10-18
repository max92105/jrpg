using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Events;

namespace Jrpg.Data.Content;

/// <summary>
/// Tells the serializer which concrete type a polymorphic content value is, from its <c>"type"</c> field. Registered
/// here rather than with attributes so Core stays free of serializer details (ADR 0002).
/// </summary>
internal static class ContentTypeDiscriminators
{
    public const string PropertyName = "type";

    public static void Register(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(EventStep))
        {
            return;
        }

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = PropertyName,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            IgnoreUnrecognizedTypeDiscriminators = false,
            DerivedTypes =
            {
                new JsonDerivedType(typeof(DialogueStep), "dialogue"),
                new JsonDerivedType(typeof(GiveItemStep), "giveItem"),
                new JsonDerivedType(typeof(SwapObjectStep), "swapObject"),
                new JsonDerivedType(typeof(WarpStep), "warp"),
            },
        };
    }
}
