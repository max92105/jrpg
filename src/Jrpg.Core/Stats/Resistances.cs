namespace Jrpg.Core.Stats;

/// <summary>
/// One resistance per damage type, in permille (ADR 0027): the share of a hit of that type taken off. Below 0 is a
/// weakness, which adds to the hit; within <see cref="FloorPercent"/>..<see cref="CapPercent"/> once clamped.
/// </summary>
public sealed record Resistances(int Blunt, int Piercing, int Slashing, int Fire, int Ice, int Lightning, int Light, int Dark)
{
    /// <summary>The most a resistance takes off, so nothing is immune: Path of Exile's default maximum.</summary>
    public const int CapPercent = 75;

    /// <summary>The deepest weakness: it at most doubles a hit (ADR 0027).</summary>
    public const int FloorPercent = -100;

    public static Resistances None { get; } = new(0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>The physical value on Blunt, Piercing and Slashing, the magical one on the five others.</summary>
    public static Resistances ByGroup(int physicalPermille, int magicalPermille) =>
        From(type => DamageTypes.IsPhysical(type) ? physicalPermille : magicalPermille);

    public int Of(DamageType type) => type switch
    {
        DamageType.Blunt => Blunt,
        DamageType.Piercing => Piercing,
        DamageType.Slashing => Slashing,
        DamageType.Fire => Fire,
        DamageType.Ice => Ice,
        DamageType.Lightning => Lightning,
        DamageType.Light => Light,
        DamageType.Dark => Dark,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown damage type."),
    };

    /// <summary>These and <paramref name="other"/> added type by type, before the limits apply.</summary>
    public Resistances Plus(Resistances other) => From(type => Of(type) + other.Of(type));

    /// <summary>Every resistance raised by <paramref name="points"/> percent, then clamped.</summary>
    public Resistances Raised(int points) => From(type => Of(type) + StatFormula.PermilleOf(points)).Clamped();

    /// <summary>Every resistance kept within the floor and the cap.</summary>
    public Resistances Clamped() =>
        From(type => Math.Clamp(Of(type), StatFormula.PermilleOf(FloorPercent), StatFormula.PermilleOf(CapPercent)));

    /// <summary>A resistance for each damage type from <paramref name="valueOf"/>, as gear gives them.</summary>
    public static Resistances From(Func<DamageType, int> valueOf) => new(
        valueOf(DamageType.Blunt),
        valueOf(DamageType.Piercing),
        valueOf(DamageType.Slashing),
        valueOf(DamageType.Fire),
        valueOf(DamageType.Ice),
        valueOf(DamageType.Lightning),
        valueOf(DamageType.Light),
        valueOf(DamageType.Dark));
}
