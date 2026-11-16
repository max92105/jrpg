using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Core.Statuses;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a whole-number field out of a file while it is 0: one added after the content was written, so every file that
/// predates it writes back byte for byte, or one that 0 means "unused" for, so an enemy the editor writes reads like a
/// hand-written one (decision 586).
/// </summary>
internal static class AbsentWhenZeroFilter
{
    private static readonly (Type Type, string Field)[] _absentByDefault =
    [
        // ADR 0040: only stacked tiles have a level above the base.
        (typeof(ObjectTile), "level"),
        // ADR 0043: most sheets stand on the frame's bottom edge.
        (typeof(FieldSpriteSheet), "feetPixels"),
        // Decision 586: only the HP condition kinds read a percent; the fallen one leaves it at 0.
        (typeof(TacticCondition), "percent"),
        // ADR 0055: a skip or a taunt has no number.
        (typeof(StatusPart), "amount"),
        // Decision 787: only an enemy with tiered skills has a Power.
        (typeof(EnemyDefinition), "power"),
        // ADR 0048: most strips land their hit on their first frame.
        (typeof(EffectStripDefinition), "hitFrame"),
        // Decision 973: only an effect on the allies in range reads its range.
        (typeof(Effect), "range"),
    ];

    public static void SkipZeroOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(typeInfo, _absentByDefault, value => value is 0);
}
