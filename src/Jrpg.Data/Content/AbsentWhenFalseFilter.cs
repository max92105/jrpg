using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Battle;
using Jrpg.Core.Skills;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a flag off by default out of a file while it is false, so an enemy or a skill the editor writes reads like a
/// hand-written one (decisions 586, 973) and only the records the developer turned it on in carry it.
/// </summary>
internal static class AbsentWhenFalseFilter
{
    private static readonly (Type Type, string Field)[] _offByDefault =
    [
        // Decisions 407, 414: only a boss keeps the party from running.
        (typeof(EnemyDefinition), "isBoss"),

        // Decisions 841, 848: most rules share the weighted draw.
        (typeof(EnemyAction), "isPriority"),

        // Decision 202: only a few skills keep their user's turn.
        (typeof(SkillDefinition), "isQuick"),
    ];

    public static void SkipFalseOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(typeInfo, _offByDefault, value => value is false);
}
