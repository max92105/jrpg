using Jrpg.Core.Effects;
using Jrpg.Data.Content;

namespace Jrpg.Data.Effects;

/// <summary>
/// The rules for <c>effects.json</c> (ADR 0056): each record a damage with a type or a heal without one, its
/// <c>perPower</c> from 1 to <see cref="MaxPerPower"/>; ids once each.
/// </summary>
internal static class EffectCatalogValidator
{
    /// <summary>
    /// A record's largest per-Power, a bound against overflow with Power up to an enemy's 10 000 (decision 818).
    /// </summary>
    public const int MaxPerPower = 1000;

    public static void Validate(string fileName, IReadOnlyList<TieredEffect> effects) =>
        new ContentListValidator<TieredEffect>(fileName, "effect", effect => effect.Id).Validate(effects, FindProblem);

    private static string? FindProblem(TieredEffect effect) => effect.Kind switch
    {
        EffectKind.Damage when effect.DamageType is null => "a damage effect needs a damageType.",
        EffectKind.RestoreHp when effect.DamageType is not null => "a restoreHp effect has no damageType.",
        EffectKind.Damage or EffectKind.RestoreHp => RangeProblems.Within("perPower", effect.PerPower, 1, MaxPerPower),
        _ => $"kind must be damage or restoreHp (was {ContentJsonOptions.EnumName(effect.Kind)}).",
    };
}
