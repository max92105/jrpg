using System.Collections.Immutable;

namespace Jrpg.Core.Stats;

/// <summary>Which kinds are the primaries and which are percents, and which kind resists which damage type.</summary>
public static class StatKinds
{
    /// <summary>The five primaries, in the order every screen lists them.</summary>
    public static ImmutableArray<StatKind> Primaries { get; } =
        [StatKind.Power, StatKind.Agility, StatKind.Intelligence, StatKind.Constitution, StatKind.Will];

    /// <summary>
    /// The stats only affixes give (ADR 0030), in the order screens list them: a base item listing one is refused at
    /// load.
    /// </summary>
    public static ImmutableArray<StatKind> AffixOnly { get; } = [StatKind.LifeSteal, StatKind.ArmorPenetration];

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

    /// <summary>
    /// Whether a sheet holds <paramref name="kind"/> in permille (ADR 0026): every percent stat; the primaries, max HP
    /// and speed are plain numbers.
    /// </summary>
    public static bool IsHeldInPermille(StatKind kind) => kind is not (
        StatKind.Power or StatKind.Agility or StatKind.Intelligence or StatKind.Constitution or StatKind.Will
        or StatKind.MaxHp or StatKind.Speed);

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
