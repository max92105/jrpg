using Jrpg.Core.Content;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Content;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The repository's content with the new game starting at a chosen place on the start map.</summary>
internal static class StartMapScenarios
{
    // Declared first: the catalogs below are built from it, in declaration order.
    private static readonly ContentCatalog _repositoryContent =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    /// <summary>One cell right of the warp cell (0, 14) to the forest, facing it.</summary>
    public static ContentCatalog NextToForestWarp { get; } = StartingAt(new Point2D(1.5, 14.5), Facing.Left);

    /// <summary>
    /// Next to the forest warp as in <see cref="NextToForestWarp"/>, but every step in the forest starts the goblin trio,
    /// with no grace steps.
    /// </summary>
    public static ContentCatalog NextToForestWarpAlwaysRolling { get; } = WithForestAlwaysRolling(NextToForestWarp);

    /// <summary>Just below the closed door, whose interact event swaps it open, facing it.</summary>
    public static ContentCatalog BelowClosedDoor { get; } = StartingAt(new Point2D(5.86, 14.6), Facing.Up);

    /// <summary>Just below the chest, whose interact event gives a potion then swaps it open, facing it.</summary>
    public static ContentCatalog BelowChest { get; } = StartingAt(new Point2D(19.5, 21.6), Facing.Up);

    /// <summary>
    /// Below the chest as in <see cref="BelowChest"/>, but the chest only gives its potion, every time, and never opens:
    /// what a well or a shop sign would do.
    /// </summary>
    public static ContentCatalog BelowRepeatableChest { get; } = WithChestSaying(BelowChest, new GiveItemStep("potion", 1));

    /// <summary>Below the chest, which now talks, in two lines, every time: the stand-in for a sign (no map has one).</summary>
    public static ContentCatalog BelowTalkingChest { get; } =
        WithChestSaying(BelowChest, new DialogueStep(new Speech(["A voice from the chest.", "It falls silent."])));

    public static InputFrame HoldingLeft { get; } =
        new(MoveIntent.FromAxes(-1, 0), ButtonSet.Empty.With(GameButton.Left), ButtonSet.Empty);

    public static TimeSpan OneUpdate { get; } = TimeSpan.FromSeconds(1.0 / 60);

    public static ExplorationState StartExploring(ContentCatalog catalog) =>
        GameSession.StartNew(catalog, new SeededRandom(1)).Exploration;

    public static ExplorationScene SceneOf(ExplorationState exploration, ContentCatalog catalog) =>
        new(exploration, catalog, new MapArt(catalog, MapView.CellPixels), new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings()));

    /// <summary>Holds Left from <see cref="NextToForestWarpAlwaysRolling"/>, through the warp, until a step requests a battle.</summary>
    /// <returns>Whether a battle was requested within 10 s.</returns>
    public static bool WalkUntilBattleRequested(ExplorationScene scene)
    {
        for (var update = 0; update < 600; update++)
        {
            if (scene.Update(OneUpdate, HoldingLeft) is BattleRequested)
            {
                return true;
            }
        }

        return false;
    }

    private static ContentCatalog StartingAt(Point2D position, Facing facing) => _repositoryContent with
    {
        NewGame = _repositoryContent.NewGame with { StartLocation = new MapLocation("start", position, facing) },
    };

    private static ContentCatalog WithForestAlwaysRolling(ContentCatalog catalog)
    {
        var forest = catalog.Maps["forest"];
        var table = new RandomEncounterTable { OneInSteps = 1, GraceSteps = 0, EncounterIds = ["goblin_trio"] };
        var maps = new Dictionary<string, MapDefinition>(catalog.Maps)
        {
            ["forest"] = new MapDefinition(forest.Layout, forest.Settings with { RandomEncounters = table }),
        };
        return catalog with { Maps = maps };
    }

    // The chest's interact event, made repeatable, runs only this step, so the chest never swaps open.
    private static ContentCatalog WithChestSaying(ContentCatalog catalog, EventStep step)
    {
        var start = catalog.Maps["start"];
        var objects = start.Layout.Objects
            .Select(placed => placed.ObjectId == "chest"
                ? placed with { Events = [placed.Events[0] with { IsOnce = false, Steps = [step] }] }
                : placed)
            .ToList();
        var maps = new Dictionary<string, MapDefinition>(catalog.Maps)
        {
            ["start"] = new MapDefinition(start.Layout with { Objects = objects }, start.Settings),
        };
        return catalog with { Maps = maps };
    }
}
