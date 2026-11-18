using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Editor.Tests.NewGame;

/// <summary>
/// The New game page tests' own new game, so no repository value is pinned: its gold, start cell and facing differ from
/// the test world's (100 gold, (2.5, 2.5) facing down), its three leaders stand in an order that isn't id order, and it
/// starts on the test world's 6x3 intro map.
/// </summary>
internal static class NewGameFixtures
{
    public static NewGameDefinition TestNewGame { get; } = new()
    {
        LeaderChoices = [new() { MemberId = "zanyr" }, new() { MemberId = "omer" }, new() { MemberId = "ragnir" }],
        StartingItems = [new InventoryEntry("potion", 5), new InventoryEntry("elixir", 1)],
        StartingGold = 250,
        StartLocation = new MapLocation("intro", new Point2D(4.5, 1.5), Facing.Left),
        FieldSpriteFile = "player.png",
    };

    /// <summary>The test world with <see cref="TestNewGame"/> as its new game.</summary>
    public static ContentCatalog World { get; } =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")) with { NewGame = TestNewGame };
}
