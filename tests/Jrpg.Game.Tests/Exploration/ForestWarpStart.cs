using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Content;
using Jrpg.Game.Input;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The repository's content with the new game starting one cell right of the start map's warp to the forest.</summary>
internal static class ForestWarpStart
{
    public static ContentCatalog Catalog { get; } = MoveStartNextToWarp(
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));

    public static InputFrame HoldingLeft { get; } =
        new(MoveIntent.FromAxes(-1, 0), ButtonSet.Empty.With(GameButton.Left), ButtonSet.Empty);

    public static TimeSpan OneUpdate { get; } = TimeSpan.FromSeconds(1.0 / 60);

    public static ExplorationState StartExploring() => GameSession.StartNew(Catalog, new SeededRandom(1)).Exploration;

    private static ContentCatalog MoveStartNextToWarp(ContentCatalog catalog) => catalog with
    {
        NewGame = catalog.NewGame with { StartLocation = new MapLocation("start", new Point2D(1.5, 14.5), Facing.Left) },
    };
}
