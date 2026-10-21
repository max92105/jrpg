using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class VictoryScreenTests
{
    private static readonly InputFrame _confirm = InputFrames.Pressing(GameButton.Confirm);
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    // Sarxian's 60 experience fill half of a 120-point level: 0.55 s.
    private readonly VictoryScreen _screen = new(
        new VictoryRewards(60, [new ExperienceGain(Sarxian(), 60, [new LevelProgressStep(1, 0, 60, 120)])]),
        RepositoryScreenText.Text,
        new UiTextures());

    [Fact]
    public void Update_ConfirmWhilePlaying_SkipsToEnd()
    {
        var request = _screen.Update(TimeSpan.FromSeconds(0.1), _confirm);

        Assert.Null(request);
        Assert.True(_screen.IsComplete);
    }

    [Fact]
    public void Update_ConfirmWhenDone_RequestsBattleFinishedOnce()
    {
        _screen.Update(TimeSpan.FromSeconds(1), default);

        var first = _screen.Update(_oneUpdate, _confirm);
        var second = _screen.Update(_oneUpdate, _confirm);

        Assert.IsType<BattleFinished>(first);
        Assert.Null(second);
    }

    private static PartyMember Sarxian() => PartyMember.Recruit(BattleScenarios.Catalog.PartyMembers["sarxian"], BattleScenarios.Catalog.Skills);
}
