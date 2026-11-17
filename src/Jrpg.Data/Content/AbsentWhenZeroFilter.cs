using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Resources;
using Jrpg.Core.Sprites;
using Jrpg.Core.Statuses;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a whole-number field out of a file while it is 0: one added after the content was written, so every file that
/// predates it writes back byte for byte, or one that 0 means "unused" for, so an enemy or a class the editor writes
/// reads like a hand-written one (decisions 586, 944).
/// </summary>
internal static class AbsentWhenZeroFilter
{
    private static readonly (Type Type, string Field)[] _absentByDefault =
    [
        // ADR 0040: only stacked tiles have a level above the base.
        (typeof(ObjectTile), "level"),
        // ADR 0043: most sheets stand on the frame's bottom edge.
        (typeof(FieldSpriteSheet), "feetPixels"),
        // Decision 586: only the HP condition kinds read a percent; the other kinds leave it at 0.
        (typeof(TacticCondition), "percent"),
        // Decision 848: only every Nth turn reads its turns.
        (typeof(TacticCondition), "turns"),
        // ADR 0055: a skip or a taunt has no number.
        (typeof(StatusPart), "amount"),
        // Decision 787: only an enemy with tiered skills has a Power.
        (typeof(EnemyDefinition), "power"),
        // ADR 0048: most strips land their hit on their first frame.
        (typeof(EffectStripDefinition), "hitFrame"),
        // Decision 973: only an effect on the allies in range reads its range.
        (typeof(Effect), "range"),
        // Decision 330: only a weapon swings.
        (typeof(EquipmentStats), "damage"),
        // Decision 944: a speed resource has neither number, and most start empty.
        (typeof(ResourceDefinition), "max"),
        (typeof(ResourceDefinition), "startsAt"),
        // Decision 944: a resource gains on a few triggers only.
        (typeof(ResourceGains), "perTurn"),
        (typeof(ResourceGains), "perAllyFallen"),
        (typeof(ResourceGains), "perHitTaken"),
        (typeof(ResourceGains), "perHeavyHit"),
        (typeof(ResourceGains), "heavyHitPercent"),
        (typeof(ResourceGains), "perEnemyFallen"),
        (typeof(ResourceGains), "perHitLanded"),
        (typeof(ResourceGains), "perBattleFought"),
    ];

    public static void SkipZeroOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(typeInfo, _absentByDefault, value => value is 0);
}
