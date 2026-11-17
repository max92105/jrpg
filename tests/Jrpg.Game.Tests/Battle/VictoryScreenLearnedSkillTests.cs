using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// The lines under the cards (decision 964). One card comes in, the bars wait 1.05 s, then each full 120-point level
// fills in 1.1 s: the two-level gain fills level 1 at 2.15 s and level 2 at 3.25 s.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class VictoryScreenLearnedSkillTests
{
    private static readonly InputFrame _confirm = InputFrames.Pressing(GameButton.Confirm);
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;
    private static readonly SkillDefinition _spark = Skill("spark", "Spark");
    private static readonly SkillDefinition _meteor = Skill("meteor", "Meteor");

    private readonly PartyMember _sarxian = VictoryScenarios.Sarxian();

    [Fact]
    public void AllOf_GainWithTwoSkills_FormatsMemberAndSkill()
    {
        var lines = LearnedSkillLine.AllOf(TwoLevelsWithTwoSkills(), _messages);

        Assert.Equal(2, lines.Count);
        Assert.All(lines, line => Assert.Contains(_sarxian.Definition.Name, line.Text));
        Assert.Contains("Spark", lines[0].Text);
        Assert.Contains("Meteor", lines[1].Text);
    }

    [Fact]
    public void AllOf_GainWithTwoSkills_WaitsOneLevelUpPerLevel()
    {
        var lines = LearnedSkillLine.AllOf(TwoLevelsWithTwoSkills(), _messages);

        Assert.Equal([1, 2], lines.Select(line => line.LevelsToFill));
    }

    [Fact]
    public void AllOf_SecondMemberLearned_NamesItsCard()
    {
        var nothing = VictoryScenarios.HalfALevel() with { Member = BattleScenarios.Recruit("jazog") };
        var rewards = new VictoryRewards(60, [nothing, TwoLevelsWithTwoSkills().Gains[0]]);

        var lines = LearnedSkillLine.AllOf(rewards, _messages);

        Assert.All(lines, line => Assert.Equal(1, line.MemberIndex));
    }

    [Fact]
    public void AllOf_NoSkillLearned_IsEmpty()
    {
        var rewards = new VictoryRewards(60, [VictoryScenarios.HalfALevel()]);

        var lines = LearnedSkillLine.AllOf(rewards, _messages);

        Assert.Empty(lines);
    }

    [Fact]
    public void OpacityOf_BeforeTheBarFills_IsZero()
    {
        var (screen, lines) = ScreenAndLines();

        Play(screen, seconds: 2.0);

        Assert.Equal(0, screen.OpacityOf(lines[0]));
    }

    [Fact]
    public void OpacityOf_JustAfterTheFill_IsFadingIn()
    {
        var (screen, lines) = ScreenAndLines();

        Play(screen, seconds: 2.25);

        Assert.InRange(screen.OpacityOf(lines[0]), 0.1f, 0.9f);
    }

    [Fact]
    public void OpacityOf_FadeOver_IsWhole()
    {
        var (screen, lines) = ScreenAndLines();

        Play(screen, seconds: 2.5);

        Assert.Equal(1, screen.OpacityOf(lines[0]));
    }

    [Fact]
    public void OpacityOf_SecondLevelSkill_WaitsForTheSecondFill()
    {
        var (screen, lines) = ScreenAndLines();

        Play(screen, seconds: 2.5);

        Assert.Equal(0, screen.OpacityOf(lines[1]));
    }

    // The second fill restarts the bar's time since its last level up; the first line must not fade in again.
    [Fact]
    public void OpacityOf_FirstLevelSkillJustAfterTheSecondFill_StaysWhole()
    {
        var (screen, lines) = ScreenAndLines();

        Play(screen, seconds: 3.3);

        Assert.Equal(1, screen.OpacityOf(lines[0]));
    }

    [Fact]
    public void OpacityOf_AfterConfirmSkip_IsWholeForAll()
    {
        var (screen, lines) = ScreenAndLines();

        screen.Update(TimeSpan.FromSeconds(0.1), _confirm);

        Assert.All(lines, line => Assert.Equal(1, screen.OpacityOf(line)));
    }

    // The level-up sound already marks the fill; a learned skill adds none.
    [Fact]
    public void Update_SkillsLearned_AsksOnlyTheLevelUpSound()
    {
        var (screen, _) = ScreenAndLines();

        screen.Update(TimeSpan.FromSeconds(5), default);

        Assert.Equal([UiSoundFiles.Battle.LevelUp], screen.Sounds);
    }

    // The same two level ups learning nothing ask the same one sound: the lines have no sound of their own.
    [Fact]
    public void Update_LevelUpsWithNothingLearned_AsksOnlyTheLevelUpSound()
    {
        var gain = TwoLevelsWithTwoSkills().Gains[0] with { LearnedSkills = [] };
        var screen = new VictoryScreen(VictoryScenarios.Spoils(gain));

        screen.Update(TimeSpan.FromSeconds(5), default);

        Assert.Equal([UiSoundFiles.Battle.LevelUp], screen.Sounds);
    }

    private static void Play(VictoryScreen screen, double seconds)
    {
        for (var update = 0; update < (int)Math.Round(seconds * 60); update++)
        {
            screen.Update(_oneUpdate, default);
        }
    }

    private (VictoryScreen Screen, IReadOnlyList<LearnedSkillLine> Lines) ScreenAndLines()
    {
        var rewards = TwoLevelsWithTwoSkills();
        return (new VictoryScreen(VictoryScenarios.Spoils(rewards)), LearnedSkillLine.AllOf(rewards, _messages));
    }

    // Sarxian goes from level 1 to 3, learning Spark at 2 and Meteor at 3.
    private VictoryRewards TwoLevelsWithTwoSkills()
    {
        LevelProgressStep[] steps = [new(1, 0, 120, 120), new(2, 0, 120, 120), new(3, 0, 10, 120)];
        var gain = new ExperienceGain(_sarxian, 250, steps, IsAtMaxLevel: false)
        {
            LearnedSkills = [new LearnedSkill(2, _spark), new LearnedSkill(3, _meteor)],
        };
        return new VictoryRewards(250, [gain]);
    }

    private static SkillDefinition Skill(string id, string name) => new()
    {
        Id = id,
        Name = name,
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy),
    };
}
