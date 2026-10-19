using Jrpg.Core.Events;
using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

public sealed class EventPresenterTests
{
    [Fact]
    public void Update_WarpStep_ContinuesOnlyOnceBlack()
    {
        var exploration = ForestWarpStart.StartExploring();
        for (var update = 0; update < 120 && !exploration.IsEventRunning; update++)
        {
            exploration.Walk(ForestWarpStart.HoldingLeft.Move, ForestWarpStart.OneUpdate);
        }

        Assert.IsType<WarpStep>(exploration.CurrentEventStep);
        var presenter = new EventPresenter(exploration);
        presenter.Update(ForestWarpStart.OneUpdate);
        presenter.Update(TimeSpan.FromSeconds(0.2));
        Assert.Equal("start", exploration.CurrentMap.Id);

        presenter.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal("forest", exploration.CurrentMap.Id);
        Assert.Equal(1f, presenter.Blackness);
        Assert.True(presenter.IsBusy);
    }
}
