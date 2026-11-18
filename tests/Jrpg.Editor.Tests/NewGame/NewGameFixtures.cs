using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Editor.Tests.Items;
using Jrpg.Editor.Tests.Party;

namespace Jrpg.Editor.Tests.NewGame;

/// <summary>
/// The New game page tests' own new game and members, so no repository value is pinned: its gold, start cell and facing
/// differ from the test world's (100 gold, (2.5, 2.5) facing down), its three leaders stand in an order that isn't id
/// order, and it starts on the test world's 6x3 intro map. Two members aren't leaders: Cato, and Zora, whose id sorts
/// first while her name sorts last. Its own items likewise: Tonic and Ember start the game, Ember stacking to 3; Salve
/// and Yarrow, whose id sorts first, don't.
/// </summary>
internal static class NewGameFixtures
{
    public static NewGameDefinition TestNewGame { get; } = new()
    {
        LeaderChoices = [new() { MemberId = "vela" }, new() { MemberId = "ilsa" }, new() { MemberId = "brom" }],
        StartingItems = [new InventoryEntry("tonic", 5), new InventoryEntry("ember", 1)],
        StartingGold = 250,
        StartLocation = new MapLocation("intro", new Point2D(4.5, 1.5), Facing.Left),
        FieldSpriteFile = "player.png",
    };

    /// <summary>
    /// The test world with the fixture's members and items and <see cref="TestNewGame"/> as its new game.
    /// </summary>
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
            Items = new Dictionary<string, ItemDefinition>
            {
                ["tonic"] = ItemNamed("tonic", "Tonic", 9),
                ["ember"] = ItemNamed("ember", "Ember", 3),
                ["acorn"] = ItemNamed("acorn", "Yarrow", 9),
                ["salve"] = ItemNamed("salve", "Salve", 9),
            },
            NewGame = TestNewGame,
        };

    private static PartyMemberDefinition MemberNamed(string id, string name) =>
        PartyMemberFixtures.Scholar with { Id = id, Name = name };

    private static ItemDefinition ItemNamed(string id, string name, int maxStack) =>
        ItemFixtures.Tonic with { Id = id, Name = name, MaxStack = maxStack };
}
