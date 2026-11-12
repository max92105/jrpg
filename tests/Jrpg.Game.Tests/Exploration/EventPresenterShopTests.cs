using Jrpg.Core.Events;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

// The chest's event opens the general store, then says a line (decision 353): the step asks once, naming the shop.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterShopTests
{
    [Fact]
    public void Update_ShopStepShown_OpensOnTheFirstUpdateOnly()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowShopChest);
        Assert.True(exploration.TryInteract());
        var presenter = EventPresenters.Over(exploration);

        presenter.Update(StartMapScenarios.OneUpdate, default);
        var first = presenter.OpeningStep;
        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal(new OpenShopStep("general_store"), first);
        Assert.Null(presenter.OpeningStep);
    }
}
