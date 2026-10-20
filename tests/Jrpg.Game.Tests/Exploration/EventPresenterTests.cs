using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterTests
{
    private static readonly InputFrame _confirmPressed =
        new(MoveIntent.None, ButtonSet.Empty.With(GameButton.Confirm), ButtonSet.Empty.With(GameButton.Confirm));

    [Fact]
    public void Update_WarpStep_ContinuesOnlyOnceBlack()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.NextToForestWarp);
        for (var update = 0; update < 120 && !exploration.IsEventRunning; update++)
        {
            exploration.Walk(StartMapScenarios.HoldingLeft.Move, StartMapScenarios.OneUpdate);
        }

        Assert.IsType<WarpStep>(exploration.CurrentEventStep);
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings());
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(TimeSpan.FromSeconds(0.2), default);
        Assert.Equal("start", exploration.CurrentMap.Id);

        presenter.Update(TimeSpan.FromSeconds(0.1), default);

        Assert.Equal("forest", exploration.CurrentMap.Id);
        Assert.Equal(1f, presenter.Blackness);
        Assert.True(presenter.IsBusy);
    }

    [Fact]
    public void Update_ConfirmOnFoundItem_ContinuesEvent()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChest);
        var chest = exploration.CurrentMap.Layout.Objects.Single(placed => placed.ObjectId == "chest");
        Assert.True(exploration.TryInteract());
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings());
        presenter.Update(StartMapScenarios.OneUpdate, default);
        Assert.IsType<GiveItemStep>(exploration.CurrentEventStep);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.False(exploration.IsEventRunning);
        Assert.Equal("chest_opened", exploration.CurrentDefinitionOf(chest).Id);
    }

    [Fact]
    public void Update_ConfirmOnLastPage_ContinuesEvent()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowTalkingChest);
        exploration.TryInteract();
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings());
        var typedOut = TimeSpan.FromSeconds(2);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(typedOut, default);
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);
        presenter.Update(typedOut, default);
        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Update_TextSpeedInstant_ShowsWholePage()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowTalkingChest);
        exploration.TryInteract();
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings { TextSpeed = TextSpeed.Instant });
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        // Each page shows whole, so one Confirm per page reads the dialogue with no time to type.
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Update_FirstUpdateOnFoundItem_WaitsEvenWithConfirmPressed()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChest);
        exploration.TryInteract();
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings());

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.IsType<GiveItemStep>(exploration.CurrentEventStep);
    }
}
