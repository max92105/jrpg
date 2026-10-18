using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jrpg.Data.Saving;

/// <summary>
/// The serializer configuration for save files: strict like content's, but kept apart so a content format change can
/// never change the save format by accident (ADR 0007).
/// </summary>
internal static class SaveJsonOptions
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
            WriteIndented = true,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        // Shared static instance: read-only so it isn't static mutable state.
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
