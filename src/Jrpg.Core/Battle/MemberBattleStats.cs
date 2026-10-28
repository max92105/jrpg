using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Battle;

/// <summary>
/// A member's battle stat line from their sheet and what they wear: kept in Battle so <see cref="PartyMember"/> doesn't
/// grow battle rules.
/// </summary>
internal static class MemberBattleStats
{
    /// <summary>
    /// The sheet's attack damage, damage percent, accuracy, dodge and resistances, attacking as the worn weapon's type,
    /// or as the rules' unarmed type without one.
    /// </summary>
    public static CombatStats Of(PartyMember member)
    {
        var sheet = member.Sheet;
        return new CombatStats(sheet.AttackDamage)
        {
            DamagePermille = sheet.DamagePermille,
            AccuracyPermille = sheet.AccuracyPermille,
            DodgePermille = sheet.DodgePermille,
            CritChancePermille = sheet.CritChancePermille,
            CritDamagePermille = sheet.CritDamagePermille,
            StatusPermille = sheet.StatusPermille,
            ExtraStatusTurns = sheet.ExtraStatusTurns,
            CrowdControlResistancePermille = sheet.CrowdControlResistancePermille,
            AttackType = member.Weapon?.DamageType ?? member.Rules.UnarmedDamageType,
            Resistances = sheet.Resistances,
        };
    }
}
