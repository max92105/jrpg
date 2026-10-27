namespace Jrpg.Core.Stats;

/// <summary>Which stat kind is the resistance to which damage type, both ways.</summary>
public static class StatKinds
{
    public static StatKind ResistanceTo(DamageType type) => type switch
    {
        DamageType.Blunt => StatKind.BluntResistance,
        DamageType.Piercing => StatKind.PiercingResistance,
        DamageType.Slashing => StatKind.SlashingResistance,
        DamageType.Fire => StatKind.FireResistance,
        DamageType.Ice => StatKind.IceResistance,
        DamageType.Lightning => StatKind.LightningResistance,
        DamageType.Light => StatKind.LightResistance,
        DamageType.Dark => StatKind.DarkResistance,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown damage type."),
    };

    /// <returns>The damage type <paramref name="kind"/> resists; null when it isn't a resistance.</returns>
    public static DamageType? TypeResistedBy(StatKind kind) => kind switch
    {
        StatKind.BluntResistance => DamageType.Blunt,
        StatKind.PiercingResistance => DamageType.Piercing,
        StatKind.SlashingResistance => DamageType.Slashing,
        StatKind.FireResistance => DamageType.Fire,
        StatKind.IceResistance => DamageType.Ice,
        StatKind.LightningResistance => DamageType.Lightning,
        StatKind.LightResistance => DamageType.Light,
        StatKind.DarkResistance => DamageType.Dark,
        _ => null,
    };
}
