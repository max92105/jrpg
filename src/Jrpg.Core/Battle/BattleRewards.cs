using Jrpg.Core.Party;
using Jrpg.Core.Progression;

namespace Jrpg.Core.Battle;

/// <summary>
/// Experience earned from defeated enemies, split evenly between the living active members.
/// Fallen members get nothing and the remainder of the split is dropped (developer decision 13).
/// </summary>
internal static class BattleRewards
{
    public static int TotalExperience(IReadOnlyList<EnemyDefinition> enemies) => enemies.Sum(enemy => enemy.ExperienceReward);

    public static int SharePerMember(int totalExperience, int recipientCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(recipientCount, 1);

        return totalExperience / recipientCount;
    }

    /// <returns>One gain per living member, in party order; empty if nobody is alive.</returns>
    public static IReadOnlyList<ExperienceGain> GrantExperience(
        IReadOnlyList<EnemyDefinition> defeated,
        IReadOnlyList<PartyMember> activeMembers,
        ExperienceTable table)
    {
        var recipients = activeMembers.Where(member => member.Vitals.IsAlive).ToList();
        if (recipients.Count == 0)
        {
            return [];
        }

        var share = SharePerMember(TotalExperience(defeated), recipients.Count);
        return [.. recipients.Select(member => GainOf(member, share, table))];
    }

    // The highest level is read after the gain, so a member who reaches it in this battle counts.
    private static ExperienceGain GainOf(PartyMember member, int share, ExperienceTable table)
    {
        var steps = member.GainExperience(share, table);
        return new ExperienceGain(member, share, steps, table.IsMaxLevel(member.Level));
    }
}
