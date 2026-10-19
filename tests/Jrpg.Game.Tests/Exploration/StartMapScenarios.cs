using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Content;
using Jrpg.Game.Input;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The repository's content with the new game starting at a chosen place on the start map.</summary>
internal static class StartMapScenarios
{
    // Declared first: the catalogs below are built from it, in declaration order.
    private static readonly ContentCatalog _repositoryContent =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    /// <summary>One cell right of the warp cell (0, 14) to the forest, facing it.</summary>
    public static ContentCatalog NextToForestWarp { get; } = StartingAt(new Point2D(1.5, 14.5), Facing.Left);

    /// <summary>Just below the closed door, whose interact event swaps it open, facing it.</summary>
    public static ContentCatalog BelowClosedDoor { get; } = StartingAt(new Point2D(5.86, 14.6), Facing.Up);

    public static InputFrame HoldingLeft { get; } =
        new(MoveIntent.FromAxes(-1, 0), ButtonSet.Empty.With(GameButton.Left), ButtonSet.Empty);

    public static TimeSpan OneUpdate { get; } = TimeSpan.FromSeconds(1.0 / 60);

    public static ExplorationState StartExploring(ContentCatalog catalog) =>
        GameSession.StartNew(catalog, new SeededRandom(1)).Exploration;

    private static ContentCatalog StartingAt(Point2D position, Facing facing) => _repositoryContent with
    {
        NewGame = _repositoryContent.NewGame with { StartLocation = new MapLocation("start", position, facing) },
    };
}
