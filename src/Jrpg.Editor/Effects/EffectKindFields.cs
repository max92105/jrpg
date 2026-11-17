using Jrpg.Core.Effects;

namespace Jrpg.Editor.Effects;

/// <summary>
/// Which typed boxes beside the targeting an effect's kind reads, for the Skills page's effect and the Items page's
/// consumable alike: a box its kind doesn't read is hidden or greyed and written empty, as Data requires.
/// </summary>
internal static class EffectKindFields
{
    /// <summary>
    /// A damage, a heal and a revive move HP by their power; a status grant, an extra turn and a rest none.
    /// </summary>
    public static bool ReadsPower(EffectKind kind) => FieldsOf(kind).ReadsPower;

    /// <summary>Only a hit meets a resistance (ADR 0027), so only a damage has a type, which Data requires.</summary>
    public static bool ReadsDamageType(EffectKind kind) => FieldsOf(kind).ReadsDamageType;

    private static (bool ReadsPower, bool ReadsDamageType) FieldsOf(EffectKind kind) =>
        kind switch
        {
            EffectKind.Damage => (true, true),
            EffectKind.RestoreHp or EffectKind.Revive => (true, false),
            EffectKind.GrantStatus or EffectKind.ReadyExtraTurn or EffectKind.Rest => (false, false),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "No effect box knows this kind."),
        };
}
