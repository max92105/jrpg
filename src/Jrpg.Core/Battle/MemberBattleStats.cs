using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Battle;

/// <summary>
/// A member's battle stat line from what they are and wear: kept in Battle so <see cref="PartyMember"/> doesn't grow
/// battle rules.
/// </summary>
internal static class MemberBattleStats
{
    /// <summary>
    /// The member's stats with gear, attacking as their weapon's type, or as the rules' unarmed type without one.
    /// </summary>
    public static CombatStats Of(PartyMember member) => member.TotalStats with
    {
        AttackType = member.Weapon?.Equipment?.DamageType ?? member.Rules.UnarmedDamageType,
    };
}
