using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

// The inn's fade (decision 257): 0.5 s out, 1.0 s of black, 0.5 s back in, then the rest's message.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterRestTests
{
    private static readonly InputFrame _confirmPressed =
        new(MoveIntent.None, ButtonSet.Empty.With(GameButton.Confirm), ButtonSet.Empty.With(GameButton.Confirm));

    [Fact]
    public void Update_RestStepStarts_ScreenGoesBlack()
    {
        var (_, presenter) = StartResting();

        presenter.Update(TimeSpan.FromSeconds(0.5), default);

        Assert.Equal(1f, presenter.Blackness);
    }

    [Fact]
    public void Update_DuringRestFade_ShowsNoMessage()
    {
        var (_, presenter) = StartResting();

        presenter.Update(TimeSpan.FromSeconds(1.2), default);

        Assert.False(presenter.IsShowingMessage);
    }

    [Fact]
    public void Update_AfterRestFade_ShowsThePartyRestsMessage()
    {
        var (exploration, presenter) = StartResting();

        presenter.Update(TimeSpan.FromSeconds(2), default);

        Assert.True(presenter.IsShowingMessage);
        Assert.Equal(0f, presenter.Blackness);
        Assert.IsType<RestStep>(exploration.CurrentEventStep);
    }

    // A guard: the running event keeps the presenter busy, and the fade mustn't end that early.
    [Fact]
    public void IsBusy_DuringRestFade_IsTrue()
    {
        var (_, presenter) = StartResting();

        presenter.Update(TimeSpan.FromSeconds(1), default);

        Assert.True(presenter.IsBusy);
    }

    [Fact]
    public void Update_SecondRest_FadesAgain()
    {
        var (exploration, presenter) = StartResting();
        presenter.Update(TimeSpan.FromSeconds(2), default);
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);
        exploration.TryInteract();
        presenter.Update(StartMapScenarios.OneUpdate, default);

        presenter.Update(TimeSpan.FromSeconds(0.5), default);

        Assert.Equal(1f, presenter.Blackness);
        Assert.False(presenter.IsShowingMessage);
    }

    // The chest's event is a single rest step; the first update starts its fade.
    private static (ExplorationState Exploration, EventPresenter Presenter) StartResting()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowRestingChest);
        exploration.TryInteract();
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());
        presenter.Update(StartMapScenarios.OneUpdate, default);
        return (exploration, presenter);
    }
}
