namespace Jrpg.Core.Stats;

/// <summary>
/// The stat conversions' rates and bases, from <c>rules.json</c> <c>stats</c> (ADR 0026). Every conversion is a percent
/// per point, written per 100 points so the numbers stay whole; bases are whole percents.
/// </summary>
/// <param name="HpPercentPer100Constitution">The percent of the class's max HP that 100 Constitution adds.</param>
/// <param name="BaseAccuracy">A member's accuracy before class and gear: their hit chance against 0 dodge.</param>
/// <param name="BaseCritChance">A member's crit chance before Agility and gear.</param>
/// <param name="BaseDodge">A member's dodge before Agility and gear.</param>
/// <param name="BaseCrowdControlResistance">A member's chance to shrug off a stun or a taunt before gear.</param>
/// <param name="DamagePercentPer100Power">The percent 100 Power adds to every raw damage and healing.</param>
/// <param name="CritDamagePercentPer100Power">The percent 100 Power adds to crit damage, over the base 150.</param>
/// <param name="CritChancePer100Agility">The crit chance percent 100 Agility adds.</param>
/// <param name="DodgePer100Agility">The dodge percent 100 Agility adds.</param>
public sealed record StatRules(
    int HpPercentPer100Constitution,
    int BaseAccuracy,
    int BaseCritChance,
    int BaseDodge,
    int BaseCrowdControlResistance,
    int DamagePercentPer100Power,
    int CritDamagePercentPer100Power,
    int CritChancePer100Agility,
    int DodgePer100Agility);
