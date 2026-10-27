namespace Jrpg.Core.Stats;

/// <summary>
/// A member's resulting numbers at their level (gear in P4): what the Status page shows and the battle fights with.
/// Derived percents are in permille (ADR 0026: <c>CritChancePermille</c> 90 is 9 %); screens show whole percents.
/// </summary>
/// <param name="Primaries">The five primaries the other numbers come from.</param>
/// <param name="MaxHp">The class's HP at the level, raised by Constitution.</param>
/// <param name="Speed">The class's speed (decision 295: not from Agility).</param>
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
    int AccuracyPermille,
    int CritChancePermille,
    int CritDamagePermille,
    int DodgePermille,
    int CrowdControlResistancePermille,
    int DamagePermille,
    Resistances Resistances,
    int StatusPermille,
    int ExtraStatusTurns);
