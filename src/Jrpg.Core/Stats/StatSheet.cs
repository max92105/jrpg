namespace Jrpg.Core.Stats;

/// <summary>
/// A member's resulting numbers at their level (gear in P4): what the Status page shows and the battle fights with.
/// Derived percents are in permille (ADR 0026: <c>CritChancePermille</c> 90 is 9 %); screens show whole percents.
/// </summary>
/// <param name="Primaries">The five primaries the other numbers come from.</param>
/// <param name="MaxHp">The class's HP at the level, raised by Constitution.</param>
/// <param name="Speed">The class's speed (decision 295: not from Agility).</param>
/// <param name="AttackDamage">
/// The basic attack's raw damage: the worn weapon's, or the unarmed one (decision 330), before Power raises it.
/// </param>
/// <param name="AccuracyPermille">The hit chance against 0 dodge.</param>
/// <param name="CritChancePermille">The chance that a hit is a crit.</param>
/// <param name="CritDamagePermille">What a crit's damage is multiplied by.</param>
/// <param name="DodgePermille">How much a foe's hit chance drops.</param>
/// <param name="CrowdControlResistancePermille">The chance to shrug off a stun or a taunt.</param>
/// <param name="DamagePermille">What every raw damage and healing the member deals is multiplied by.</param>
/// <param name="Resistances">One per damage type: Constitution raises the physical ones, Will the magical ones.</param>
/// <param name="StatusPermille">What a status the member grants has its power multiplied by.</param>
/// <param name="ExtraStatusTurns">The turns added to a status the member grants, except Stun (decision 317).</param>
public sealed record StatSheet(
    PrimaryStats Primaries,
    int MaxHp,
    int Speed,
    int AttackDamage,
    int AccuracyPermille,
    int CritChancePermille,
    int CritDamagePermille,
    int DodgePermille,
    int CrowdControlResistancePermille,
    int DamagePermille,
    Resistances Resistances,
    int StatusPermille,
    int ExtraStatusTurns)
{
    /// <summary>
    /// The share of the HP each landed hit of an attack or a damaging skill took that heals the member.
    /// </summary>
    public int LifeStealPermille { get; init; }

    /// <summary>The percentage points the member's hits take off a positive resistance (decision 364).</summary>
    public int ArmorPenetrationPermille { get; init; }

    /// <summary>The chance that a landed hit on the member is blocked (decision 348).</summary>
    public int BlockChancePermille { get; init; }

    /// <summary>
    /// How much the member raises drop chances and the tiers above Common, summed over the active members (decision
    /// 349).
    /// </summary>
    public int DropLuckPermille { get; init; }

    /// <summary>
    /// The number a screen shows for <paramref name="kind"/>: whole points for a primary, max HP and speed, permille
    /// for the percent kinds. The one place that reads a sheet by kind, so screens list stats without a switch.
    /// </summary>
    public int ValueOf(StatKind kind) =>
        StatKinds.TypeResistedBy(kind) is { } type ? Resistances.Of(type) : DerivedOrPrimary(kind);

    private int DerivedOrPrimary(StatKind kind) => kind switch
    {
        StatKind.MaxHp => MaxHp,
        StatKind.Speed => Speed,
        StatKind.Accuracy => AccuracyPermille,
        StatKind.CritChance => CritChancePermille,
        StatKind.CritDamage => CritDamagePermille,
        StatKind.Dodge => DodgePermille,
        StatKind.CrowdControlResistance => CrowdControlResistancePermille,
        _ => AffixStatOf(kind),
    };

    // Apart from the derived stats so each switch stays under the build's complexity limit.
    private int AffixStatOf(StatKind kind) => kind switch
    {
        StatKind.LifeSteal => LifeStealPermille,
        StatKind.ArmorPenetration => ArmorPenetrationPermille,
        StatKind.BlockChance => BlockChancePermille,
        StatKind.DropLuck => DropLuckPermille,
        _ => Primaries.ValueOf(kind),
    };
}
