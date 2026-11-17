using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>
/// A member of a class at a level wearing nothing: the one reading of that rule (decision 948), so a page showing a
/// class's numbers, such as the editor's Levels tab, never copies <see cref="MemberSheet"/>'s call.
/// </summary>
public static class ClassSheet
{
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is below 1.</exception>
    public static StatSheet At(ClassDefinition characterClass, int level, StatRules rules) =>
        MemberSheet.Of(ClassGrowth.StatsAt(characterClass, level), StatBonuses.None, weaponDamage: null, rules);
}
