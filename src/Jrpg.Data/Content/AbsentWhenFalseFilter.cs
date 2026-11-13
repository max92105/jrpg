using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Battle;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a flag off by default out of a file while it is false, so an enemy the editor writes reads like a
/// hand-written one (decision 586) and only the files the developer turned it on in carry it.
/// </summary>
internal static class AbsentWhenFalseFilter
{
    private static readonly (Type Type, string Field)[] _offByDefault =
    [
        // Decisions 407, 414: only a boss keeps the party from running.
        (typeof(EnemyDefinition), "isBoss"),
    ];

    public static void SkipFalseOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(typeInfo, _offByDefault, value => value is false);
}
