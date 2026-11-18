using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Battle;

/// <summary>
/// The battle stat line of a member of a class at a level wearing nothing, attacking unarmed (decision 993): the
/// editor's balance table reads it, so its numbers are the battle's own.
/// </summary>
public static class ClassBattleStats
{
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is below 1.</exception>
    public static CombatStats At(ClassDefinition characterClass, int level, StatRules rules) =>
        MemberBattleStats.Of(ClassSheet.At(characterClass, level, rules), rules.UnarmedDamageType);
}
