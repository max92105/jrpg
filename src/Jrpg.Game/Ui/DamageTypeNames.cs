using Jrpg.Core.Stats;

namespace Jrpg.Game.Ui;

/// <summary>Each damage type's short name in ui.json (ADR 0010), as the Status page lists resistances.</summary>
internal static class DamageTypeNames
{
    public static string IdOf(DamageType type) => type switch
    {
        DamageType.Blunt => MessageIds.DamageTypeBlunt,
        DamageType.Piercing => MessageIds.DamageTypePiercing,
        DamageType.Slashing => MessageIds.DamageTypeSlashing,
        DamageType.Fire => MessageIds.DamageTypeFire,
        DamageType.Ice => MessageIds.DamageTypeIce,
        DamageType.Lightning => MessageIds.DamageTypeLightning,
        DamageType.Light => MessageIds.DamageTypeLight,
        DamageType.Dark => MessageIds.DamageTypeDark,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A damage type with no name in ui.json."),
    };
}
