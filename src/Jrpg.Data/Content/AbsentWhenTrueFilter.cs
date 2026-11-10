using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Maps;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a true-by-default field added after the content was written out of a file while it is true, so every file
/// that predates it writes back byte for byte and only the files the developer turned it off in carry it.
/// </summary>
internal static class AbsentWhenTrueFilter
{
    private static readonly (Type Type, string Field)[] _addedLater =
    [
        // Decision 528: every map shows the entry banner unless turned off.
        (typeof(MapSettings), "showsBanner"),
    ];

    public static void SkipTrueOptionalFields(JsonTypeInfo typeInfo) => OptionalFieldFilter.SkipWhen(typeInfo, _addedLater, value => value is true);
}
