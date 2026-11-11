using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Jrpg.Data.Battle;
using Jrpg.Data.Items;

namespace Jrpg.Data.Content;

/// <summary>
/// The one serializer configuration for content files. Strict, so a typo or missing field fails at load time; and the
/// one spelling of an enum value, the converter's, for messages that name one.
/// </summary>
internal static class ContentJsonOptions
{
    private static JsonNamingPolicy EnumNaming => JsonNamingPolicy.CamelCase;

    public static JsonSerializerOptions Strict { get; } = CreateStrict();

    /// <summary>
    /// An enum value's name as content files spell it, the converter's spelling, for messages that name a value.
    /// </summary>
    public static string EnumName<T>(T value) where T : struct, Enum => EnumNaming.ConvertName(value.ToString());

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
            TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { ContentTypeDiscriminators.Register, ComputedPropertyFilter.RemoveComputedProperties, AbsentWhenNullFilter.SkipNullOptionalFields, AbsentWhenZeroFilter.SkipZeroOptionalFields, AbsentWhenTrueFilter.SkipTrueOptionalFields } },
        };
        options.Converters.Add(new JsonStringEnumConverter(EnumNaming, allowIntegerValues: false));
        options.Converters.Add(new StatBonusesConverter());
        options.Converters.Add(new ResistancesConverter());

        // Shared static instance: read-only so it isn't static mutable state.
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
