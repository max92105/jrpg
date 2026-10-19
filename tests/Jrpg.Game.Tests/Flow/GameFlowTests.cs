using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Game.Flow;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Flow;

public sealed class GameFlowTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Constructor_RepositoryContent_StartsANewGameOnTheStartMap()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text);

        Assert.Equal("start", flow.Session.Exploration.Location.MapId);
    }

    [Fact]
    public void Update_TwoDeltas_AddsBothToPlayTime()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text);

        flow.Update(TimeSpan.FromSeconds(0.5), default);
        flow.Update(TimeSpan.FromSeconds(0.25), default);

        Assert.Equal(TimeSpan.FromSeconds(0.75), flow.Session.PlayTime);
    }

    [Fact]
    public void Update_HoldingRight_MovesPlayerRight()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text);
        var start = flow.Session.Exploration.PlayerPosition;
        var holdingRight = new InputFrame(MoveIntent.FromAxes(1, 0), ButtonSet.Empty.With(GameButton.Right), ButtonSet.Empty);

        flow.Update(TimeSpan.FromSeconds(0.1), holdingRight);

        var position = flow.Session.Exploration.PlayerPosition;
        Assert.True(position.X > start.X, $"Expected the player right of {start.X}, at {position.X}.");
        Assert.Equal(start.Y, position.Y);
        Assert.Equal(Facing.Right, flow.Session.Exploration.PlayerFacing);
    }
}
