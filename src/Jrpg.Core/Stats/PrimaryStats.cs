namespace Jrpg.Core.Stats;

/// <summary>
/// The five primaries (ADR 0026), each a dimension a build can lean on (decision 296): Power scales every hit and heal,
/// Agility crit chance and dodge, Intelligence what a skill's statuses do, Constitution max HP and the physical
/// resistances, Will the class resource and the magical resistances. A class gives them at level 1 and grows them.
/// </summary>
public sealed record PrimaryStats(int Power, int Agility, int Intelligence, int Constitution, int Will)
{
    /// <summary>All five at 0: a class or a growth that gives none.</summary>
    public static PrimaryStats None { get; } = new(0, 0, 0, 0, 0);

    /// <summary>These and <paramref name="other"/> added primary by primary, as gear adds to the class's.</summary>
    public PrimaryStats Plus(PrimaryStats other) =>
        new(Power + other.Power, Agility + other.Agility, Intelligence + other.Intelligence, Constitution + other.Constitution, Will + other.Will);

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> isn't a primary.</exception>
    public int ValueOf(StatKind kind) => kind switch
    {
        StatKind.Power => Power,
        StatKind.Agility => Agility,
        StatKind.Intelligence => Intelligence,
        StatKind.Constitution => Constitution,
        StatKind.Will => Will,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a primary."),
    };
}
