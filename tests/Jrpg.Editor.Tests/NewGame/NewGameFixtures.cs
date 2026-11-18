using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Editor.Tests.Party;

namespace Jrpg.Editor.Tests.NewGame;

/// <summary>
/// The New game page tests' own new game and members, so no repository value is pinned: its gold, start cell and facing
/// differ from the test world's (100 gold, (2.5, 2.5) facing down), its three leaders stand in an order that isn't id
/// order, and it starts on the test world's 6x3 intro map. Two members aren't leaders: Cato, and Zora, whose id sorts
/// first while her name sorts last.
/// </summary>
internal static class NewGameFixtures
{
    public static NewGameDefinition TestNewGame { get; } = new()
    {
        LeaderChoices = [new() { MemberId = "vela" }, new() { MemberId = "ilsa" }, new() { MemberId = "brom" }],
        StartingItems = [new InventoryEntry("potion", 5), new InventoryEntry("elixir", 1)],
        StartingGold = 250,
        StartLocation = new MapLocation("intro", new Point2D(4.5, 1.5), Facing.Left),
        FieldSpriteFile = "player.png",
    };

    /// <summary>The test world with the fixture's members and <see cref="TestNewGame"/> as its new game.</summary>
    public static ContentCatalog World { get; } =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")) with
        {
            PartyMembers = new Dictionary<string, PartyMemberDefinition>
            {
                ["vela"] = MemberNamed("vela", "Vela"),
                ["ilsa"] = MemberNamed("ilsa", "Ilsa"),
                ["brom"] = MemberNamed("brom", "Brom"),
                ["cato"] = MemberNamed("cato", "Cato"),
                ["abel"] = MemberNamed("abel", "Zora"),
            },
            NewGame = TestNewGame,
        };

    private static PartyMemberDefinition MemberNamed(string id, string name) =>
        PartyMemberFixtures.Scholar with { Id = id, Name = name };
}
