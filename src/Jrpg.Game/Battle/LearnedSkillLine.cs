using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>One skill a member learned, as the victory screen shows it under the cards (decision 964).</summary>
/// <param name="Text">"Omer learned Meteor.", formatted once.</param>
/// <param name="MemberIndex">The member's card: its place in the rewards' gains.</param>
/// <param name="LevelsToFill">
/// How many of that member's level ups the bar must have filled before the line shows.
/// </param>
internal sealed record LearnedSkillLine(string Text, int MemberIndex, int LevelsToFill)
{
    /// <summary>
    /// Every skill learned in <paramref name="rewards"/>, in party order, then in the order Core lists a member's: by
    /// level, then the class's order.
    /// </summary>
    public static IReadOnlyList<LearnedSkillLine> AllOf(VictoryRewards rewards, UiMessages messages)
    {
        var lines = new List<LearnedSkillLine>();
        for (var index = 0; index < rewards.Gains.Count; index++)
        {
            var gain = rewards.Gains[index];
            foreach (var learned in gain.LearnedSkills)
            {
                lines.Add(new(TextOf(gain, learned, messages), index, LevelsToFillFor(gain, learned)));
            }
        }

        return lines;
    }

    private static string TextOf(ExperienceGain gain, LearnedSkill learned, UiMessages messages) =>
        messages.Format(
            MessageIds.BattleSkillLearned,
            new MessageArgument("name", gain.Member.Definition.Name),
            new MessageArgument("skill", learned.Skill.Name));

    // A skill came at a level up, so the gain has steps; the first names the level the member had before it.
    private static int LevelsToFillFor(ExperienceGain gain, LearnedSkill learned) => learned.Level - gain.Steps[0].Level;
}
