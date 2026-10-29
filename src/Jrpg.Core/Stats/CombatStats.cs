namespace Jrpg.Core.Stats;

/// <summary>
/// A combatant's battle stat line. A class, not a struct: the stats later slices add have neutral defaults (accuracy
/// 100 %, crit damage 150 %) that a struct's <c>default</c> would silently zero.
/// </summary>
/// <param name="AttackDamage">The basic attack's raw damage: the weapon's, the unarmed one, or an enemy's.</param>
public sealed record CombatStats(int AttackDamage)
{
    /// <summary>
    /// What the combatant's raw damage and healing are multiplied by, in permille: Power's for a member (decision 296),
    /// 1000 for an enemy.
    /// </summary>
    public int DamagePermille { get; init; } = 1000;

    /// <summary>
    /// The hit chance against 0 dodge, in permille (decision 297): a member's from the sheet, an enemy's from content.
    /// </summary>
    public int AccuracyPermille { get; init; } = 1000;

    /// <summary>How much a foe's hit chance drops, in permille.</summary>
    public int DodgePermille { get; init; }

    /// <summary>
    /// The chance that a landed hit is a crit, in permille: Agility's for a member, content's for an enemy.
    /// </summary>
    public int CritChancePermille { get; init; }

    /// <summary>
    /// What a crit's power is multiplied by, in permille: the base 150 % raised by Power for a member (decision 300).
    /// </summary>
    public int CritDamagePermille { get; init; } = StatFormula.PermilleOf(StatFormula.BaseCritDamagePercent);

    /// <summary>
    /// What a status the combatant grants has its power multiplied by, in permille: Intelligence's for a member.
    /// </summary>
    public int StatusPermille { get; init; } = 1000;

    /// <summary>The turns a status the combatant grants lasts longer, except Stun (decision 317).</summary>
    public int ExtraStatusTurns { get; init; }

    /// <summary>The chance to shrug off a stun or a taunt, in permille (decisions 302 and 324).</summary>
    public int CrowdControlResistancePermille { get; init; }

    /// <summary>What the combatant's basic attack hits as (ADR 0027).</summary>
    public DamageType AttackType { get; init; } = DamageType.Blunt;

    /// <summary>The share of each damage type's hits taken off (ADR 0027).</summary>
    public Resistances Resistances { get; init; } = Resistances.None;

    /// <summary>
    /// The share of the HP each landed hit of an attack or a damaging skill took that heals the combatant, in permille:
    /// a member's from their gear's affixes (decision 346), 0 for an enemy.
    /// </summary>
    public int LifeStealPermille { get; init; }

    /// <summary>
    /// The percentage points the combatant's hits take off a positive resistance, in permille: a member's from their
    /// gear's affixes (decision 364), 0 for an enemy.
    /// </summary>
    public int ArmorPenetrationPermille { get; init; }

    /// <summary>
    /// The chance that a landed hit on the combatant is blocked, in permille: a member's from their gear's affixes
    /// (decision 348), 0 for an enemy.
    /// </summary>
    public int BlockChancePermille { get; init; }
}
