using System.Collections;
using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Battle;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a list empty by default out of a file while it is empty, so an enemy the editor writes reads like a
/// hand-written one (decision 586): most enemies only attack and have no phases (ADR 0034).
/// </summary>
internal static class AbsentWhenEmptyFilter
{
    private static readonly (Type Type, string Field)[] _emptyByDefault =
    [
        (typeof(EnemyDefinition), "tactics"),
        (typeof(EnemyDefinition), "phases"),
    ];

    public static void SkipEmptyOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(typeInfo, _emptyByDefault, value => value is ICollection { Count: 0 });
}
