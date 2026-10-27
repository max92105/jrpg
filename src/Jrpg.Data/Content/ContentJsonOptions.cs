using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Jrpg.Data.Battle;
using Jrpg.Data.Items;

namespace Jrpg.Data.Content;

/// <summary>The one serializer configuration for content files. Strict, so a typo or missing field fails at load time.</summary>
internal static class ContentJsonOptions
{
    public static JsonSerializerOptions Strict { get; } = CreateStrict();

    private static JsonSerializerOptions CreateStrict()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            AllowDuplicateProperties = false,

            // Hand-written content may put "type" anywhere in a step, not only first.
            AllowOutOfOrderMetadataProperties = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { ContentTypeDiscriminators.Register, ComputedPropertyFilter.RemoveComputedProperties, AbsentWhenNullFilter.SkipNullOptionalFields } },
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        options.Converters.Add(new StatBonusesConverter());
        options.Converters.Add(new ResistancesConverter());

        // Shared static instance: read-only so it isn't static mutable state.
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
