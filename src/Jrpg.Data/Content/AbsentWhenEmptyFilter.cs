using System.Collections;
using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Sprites;
using Jrpg.Core.Stats;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a list empty by default out of a file while it is empty, so an enemy the editor writes reads like a
/// hand-written one (decision 586): most enemies only attack and have no phases (ADR 0034); a field sprite facing that
/// mirrors another has no cells (ADR 0054); a skill or its effect leaves out a list it has no use for (decision 973);
/// gear adding no stat leaves out its bonuses, a <see cref="StatBonuses"/> naming no kind (decision 862); an item that
/// isn't unique leaves out its fixed affixes (ADR 0058).
/// </summary>
internal static class AbsentWhenEmptyFilter
{
    private static readonly (Type Type, string Field)[] _emptyByDefault =
    [
        (typeof(EnemyDefinition), "tactics"),
        (typeof(EnemyDefinition), "phases"),
        (typeof(FacingSequence), "cells"),

        // ADR 0019: a skill may cost nothing.
        (typeof(SkillDefinition), "resourceChanges"),

        // ADR 0019: most skills need no minimum held.
        (typeof(SkillDefinition), "requirements"),

        // ADR 0055: most effects grant no status.
        (typeof(Effect), "statuses"),

        // ADR 0026: most weapons add no stat.
        (typeof(EquipmentStats), "bonuses"),

        // ADR 0058: only a unique has fixed affixes.
        (typeof(ItemDefinition), "fixedAffixes"),
    ];

    // StatBonuses isn't a collection, so it is tested apart.
    public static void SkipEmptyOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(
            typeInfo, _emptyByDefault, value => value is ICollection { Count: 0 } or StatBonuses { Values.Count: 0 });
}
