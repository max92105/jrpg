using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Exploration;

// The chest's event opens the Forge, then says a line (decision 353).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterForgeTests
{
    [Fact]
    public void Update_ForgeStepShown_OpensTheForgeOnTheFirstUpdateOnly()
    {
        var (_, presenter) = StartAtTheForge();

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Null(presenter.OpeningStep);
    }

    // After the Forge closes and the event ends, talking again runs a new Forge step, which opens it again.
    [Fact]
    public void Update_SameEventRunAgain_OpensTheForgeAgain()
    {
        var (exploration, presenter) = StartAtTheForge();
        exploration.ContinueEvent();
        presenter.Update(StartMapScenarios.OneUpdate, default);
        exploration.ContinueEvent();
        presenter.Update(StartMapScenarios.OneUpdate, default);
        Assert.False(exploration.IsEventRunning);

        Assert.True(exploration.TryInteract());
        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.IsType<OpenForgeStep>(presenter.OpeningStep);
    }

    // Two Forge steps in a row: the second is another step, so the Forge opens again rather than the event waiting.
    [Fact]
    public void Update_SecondForgeStepRightAfter_OpensTheForgeAgain()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowDoubleForgeChest);
        Assert.True(exploration.TryInteract());
        var presenter = new EventPresenter(exploration, new EventLook(RepositoryScreenText.Text, new UiTextures()), new PlayerSettings(), new VoiceChannel());
        presenter.Update(StartMapScenarios.OneUpdate, default);
        exploration.ContinueEvent();

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.IsType<OpenForgeStep>(presenter.OpeningStep);
    }

    // The first update shows the Forge step and asks for the Forge.
    private static (ExplorationState Exploration, EventPresenter Presenter) StartAtTheForge()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowForgeChest);
        Assert.True(exploration.TryInteract());
        var presenter = new EventPresenter(exploration, new EventLook(RepositoryScreenText.Text, new UiTextures()), new PlayerSettings(), new VoiceChannel());
        presenter.Update(StartMapScenarios.OneUpdate, default);
        Assert.IsType<OpenForgeStep>(presenter.OpeningStep);
        return (exploration, presenter);
    }
}
