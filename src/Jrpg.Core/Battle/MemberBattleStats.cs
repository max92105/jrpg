using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Battle;

/// <summary>
/// A member's battle stat line from their sheet and what they wear: kept in Battle so <see cref="PartyMember"/> doesn't
/// grow battle rules. <see cref="ClassBattleStats"/> reads a class's line through the same mapping.
/// </summary>
internal static class MemberBattleStats
{
    /// <summary>
    /// The member's sheet, attacking as the worn weapon's type, or as the rules' unarmed type without one.
    /// </summary>
    public static CombatStats Of(PartyMember member) =>
        Of(member.Sheet, member.Weapon?.DamageType ?? member.Rules.UnarmedDamageType);

    /// <summary>
    /// The sheet's attack damage, Power, damage percent, damage bonus (the damage modifiers' starting sum), accuracy,
    /// dodge, resistances, lifesteal, penetration and block, attacking as <paramref name="attackType"/>.
    /// </summary>
    public static CombatStats Of(StatSheet sheet, DamageType attackType) => new(sheet.AttackDamage)
    {
        Power = sheet.Primaries.Power,
        DamagePermille = sheet.DamagePermille,
        DamageModifierPermille = sheet.DamageBonusPermille,
        AccuracyPermille = sheet.AccuracyPermille,
        DodgePermille = sheet.DodgePermille,
        CritChancePermille = sheet.CritChancePermille,
        CritDamagePermille = sheet.CritDamagePermille,
        StatusPermille = sheet.StatusPermille,
        ExtraStatusTurns = sheet.ExtraStatusTurns,
        CrowdControlResistancePermille = sheet.CrowdControlResistancePermille,
        AttackType = attackType,
        Resistances = sheet.Resistances,
        LifeStealPermille = sheet.LifeStealPermille,
        ArmorPenetrationPermille = sheet.ArmorPenetrationPermille,
        BlockChancePermille = sheet.BlockChancePermille,
    };
}
