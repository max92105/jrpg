using Jrpg.Core.Progression;
using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// A full level fills in 1.1 s; a step takes its share of that, but at least 0.2 s.
public sealed class ExperienceBarAnimationTests
{
    // Level 1 from 50 of 100 to the next level (0.55 s), level 2 in full (1.1 s), then 30 of 150 into level 3 (0.22 s).
    private static readonly LevelProgressStep[] _twoLevelUps =
    [
        new(Level: 1, FromExperience: 50, ToExperience: 100, ExperienceToNextLevel: 100),
        new(Level: 2, FromExperience: 0, ToExperience: 120, ExperienceToNextLevel: 120),
        new(Level: 3, FromExperience: 0, ToExperience: 30, ExperienceToNextLevel: 150),
    ];

    [Fact]
    public void Update_TwoLevelUps_PassesThroughBothSteps()
    {
        var bar = new ExperienceBarAnimation(_twoLevelUps, finalLevel: 3);

        bar.Update(TimeSpan.FromSeconds(0.55 + 0.55));

        Assert.Equal(2, bar.Level);
        Assert.Equal(60, bar.Experience);
        Assert.Equal(120, bar.ExperienceToNextLevel);
        Assert.Equal(1, bar.LevelUps);

        bar.Update(TimeSpan.FromSeconds(0.55 + 0.22));

        Assert.True(bar.IsComplete);
        Assert.Equal((3, 30, 150, 2), (bar.Level, bar.Experience, bar.ExperienceToNextLevel, bar.LevelUps));
    }

    [Fact]
    public void Skip_WhilePlaying_JumpsToFinalStep()
    {
        var bar = new ExperienceBarAnimation(_twoLevelUps, finalLevel: 3);
        bar.Update(TimeSpan.FromSeconds(0.1));

        bar.Skip();

        Assert.True(bar.IsComplete);
        Assert.Equal((3, 30, 150, 2), (bar.Level, bar.Experience, bar.ExperienceToNextLevel, bar.LevelUps));
    }

    [Fact]
    public void Update_GainEndsOnLevelBoundary_ShowsEmptyBarAtNewLevel()
    {
        var bar = new ExperienceBarAnimation([new LevelProgressStep(1, 50, 100, 100)], finalLevel: 2);

        bar.Update(TimeSpan.FromSeconds(1));

        Assert.True(bar.IsComplete);
        Assert.Equal((2, 0, 1), (bar.Level, bar.Experience, bar.LevelUps));
    }

    [Fact]
    public void Update_PastLevelUp_CountsTimeSinceIt()
    {
        var bar = new ExperienceBarAnimation(_twoLevelUps, finalLevel: 3);
        bar.Update(TimeSpan.FromSeconds(0.55));

        bar.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal(TimeSpan.FromSeconds(0.1), bar.SinceLastLevelUp);
    }

    [Fact]
    public void Skip_PastLevelUps_ShowsThemWithoutPop()
    {
        var bar = new ExperienceBarAnimation(_twoLevelUps, finalLevel: 3);

        bar.Skip();

        Assert.Null(bar.SinceLastLevelUp);
    }

    [Fact]
    public void IsComplete_NoSteps_IsTrue()
    {
        var bar = new ExperienceBarAnimation([], finalLevel: 4);

        Assert.True(bar.IsComplete);
        Assert.Equal(4, bar.Level);
    }

    [Fact]
    public void Update_SmallGain_TakesMinimumStepTime()
    {
        var bar = new ExperienceBarAnimation([new LevelProgressStep(1, 0, 10, 100)], finalLevel: 1);

        bar.Update(TimeSpan.FromSeconds(0.1));

        Assert.False(bar.IsComplete);
        Assert.Equal(5, bar.Experience);
    }
}
