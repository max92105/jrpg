using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

public sealed class VictoryScreenTests
{
    private static readonly InputFrame _confirm = InputFrames.Pressing(GameButton.Confirm);
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    // One card comes in, the bars wait 1.05 s, then Sarxian's 60 experience fill half of a 120-point level in 0.55 s.
    private readonly VictoryScreen _screen = new(
        new VictoryRewards(60, [new ExperienceGain(Sarxian(), 60, [new LevelProgressStep(1, 0, 60, 120)], IsAtMaxLevel: false)]));

    [Fact]
    public void Update_ConfirmWhilePlaying_SkipsToEnd()
    {
        var request = _screen.Update(TimeSpan.FromSeconds(0.1), _confirm);

        Assert.Null(request);
        Assert.True(_screen.IsComplete);
    }

    [Fact]
    public void Update_DuringEntrance_BarsWait()
    {
        _screen.Update(TimeSpan.FromSeconds(1), default);

        Assert.Equal(0, _screen.Bars[0].Experience);
    }

    [Fact]
    public void Update_Done_ShowsNextHint()
    {
        _screen.Update(TimeSpan.FromSeconds(1), default);

        _screen.Update(TimeSpan.FromSeconds(1), default);

        Assert.True(_screen.IsComplete);
    }

    [Fact]
    public void Update_ConfirmWhenDone_RequestsBattleFinishedOnce()
    {
        _screen.Update(TimeSpan.FromSeconds(3), default);

        var first = _screen.Update(_oneUpdate, _confirm);
        var second = _screen.Update(_oneUpdate, _confirm);

        Assert.IsType<BattleFinished>(first);
        Assert.Null(second);
    }

    // Already at the highest level: Core gives no steps, so the bar is complete from the start.
    [Fact]
    public void IsAtMaxLevel_MemberAlreadyAtMaxLevel_IsTrue()
    {
        var screen = new VictoryScreen(new VictoryRewards(60, [new ExperienceGain(Sarxian(), 60, [], IsAtMaxLevel: true)]));

        Assert.True(screen.IsAtMaxLevel(0));
    }

    // The gain fills the last level, so the card shows "Max" only once the bar has filled it.
    [Fact]
    public void IsAtMaxLevel_ReachesMaxLevelWhileFilling_FalseUntilTheBarCompletes()
    {
        var screen = new VictoryScreen(
            new VictoryRewards(60, [new ExperienceGain(Sarxian(), 60, [new LevelProgressStep(1, 60, 120, 120)], IsAtMaxLevel: true)]));
        screen.Update(TimeSpan.FromSeconds(1.1), default);
        Assert.False(screen.IsAtMaxLevel(0));

        screen.Update(TimeSpan.FromSeconds(1), default);

        Assert.True(screen.IsAtMaxLevel(0));
    }

    [Fact]
    public void IsAtMaxLevel_BelowMaxLevel_IsFalse()
    {
        _screen.Update(TimeSpan.FromSeconds(3), default);

        Assert.False(_screen.IsAtMaxLevel(0));
    }

    private static PartyMember Sarxian() => PartyMember.Recruit(BattleScenarios.Catalog.PartyMembers["sarxian"], BattleScenarios.Catalog.Skills);
}
