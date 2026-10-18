using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class NewGameValidatorTests
{
    private static readonly ItemDefinition _potion = new()
    {
        Id = "potion",
        Name = "Potion",
        Description = "",
        IconFile = "Consumable.png",
        MaxStack = 99,
        IsUsableFromMenu = true,
    };

    private static readonly MapDefinition _start = new(
        new MapLayout { Id = "start", Width = 45, Height = 45, Layers = [], Objects = [] },
        new MapSettings { Id = "start", DisplayName = "start" });

    private static readonly NewGameReferences _references = new(
        MemberIds: new HashSet<string> { "sarxian", "jazog", "teelle" },
        Items: new Dictionary<string, ItemDefinition> { ["potion"] = _potion },
        Party: new PartyRules(MaxActiveMembers: 2),
        Maps: new Dictionary<string, MapDefinition> { ["start"] = _start });

    private static readonly NewGameDefinition _newGame = new()
    {
        RecruitedMemberIds = ["sarxian", "jazog"],
        ActiveMemberIds = ["sarxian"],
        StartingItems = [new InventoryEntry("potion", 5)],
        StartLocation = new MapLocation("start", new Point2D(10, 3), Facing.Down),
        FieldSpriteFile = "player.png",
    };

    public static TheoryData<NewGameDefinition, string> InvalidNewGames => new()
    {
        { _newGame with { RecruitedMemberIds = [] }, "recruitedMemberIds must list at least one party member." },
        { _newGame with { RecruitedMemberIds = ["sarxian", "nobody"] }, "recruitedMemberIds has unknown party member id 'nobody'." },
        { _newGame with { RecruitedMemberIds = ["sarxian", "sarxian"] }, "recruitedMemberIds lists 'sarxian' more than once." },
        { _newGame with { RecruitedMemberIds = ["sarxian", null!] }, "recruitedMemberIds and activeMemberIds must not contain a null or empty id." },
        { _newGame with { ActiveMemberIds = [null!] }, "recruitedMemberIds and activeMemberIds must not contain a null or empty id." },
        { _newGame with { ActiveMemberIds = [] }, "activeMemberIds must list 1 to 2 members, leader first (was 0)." },
        { _newGame with { ActiveMemberIds = ["teelle"] }, "activeMemberIds has 'teelle', who isn't in recruitedMemberIds." },
        { _newGame with { ActiveMemberIds = ["sarxian", "sarxian"] }, "activeMemberIds lists 'sarxian' more than once." },
        { _newGame with { StartingItems = [null!] }, "startingItems has a null entry." },
        { _newGame with { StartingItems = [new InventoryEntry("potion", 0)] }, "startingItems 'potion': quantity must be within 1..99 (was 0)." },
        { _newGame with { StartingItems = [new InventoryEntry("potion", 100)] }, "startingItems 'potion': quantity must be within 1..99 (was 100)." },
        { _newGame with { StartingItems = [new InventoryEntry("potion", 1), new InventoryEntry("potion", 2)] }, "startingItems lists 'potion' more than once." },
        { _newGame with { StartLocation = _newGame.StartLocation with { MapId = "castle" } }, "startLocation.mapId has unknown map id 'castle'." },
        { _newGame with { StartLocation = _newGame.StartLocation with { Position = new Point2D(45, 3) } }, "startLocation.position (45, 3) must be inside map 'start' (45x45)." },
        { _newGame with { StartLocation = _newGame.StartLocation with { Position = new Point2D(-0.5, 3) } }, "startLocation.position (-0.5, 3) must be inside map 'start' (45x45)." },
        { _newGame with { FieldSpriteFile = " " }, "fieldSpriteFile must name the map sprite's file." },
    };

    [Fact]
    public void Validate_ValidNewGame_DoesNotThrow()
    {
        var exception = Record.Exception(() => NewGameValidator.Validate("new-game.json", _newGame, _references));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_UnknownItemId_ThrowsNamingTheId()
    {
        var newGame = _newGame with { StartingItems = [new InventoryEntry("megalixir", 1)] };

        var exception = Assert.Throws<ContentValidationException>(() => NewGameValidator.Validate("new-game.json", newGame, _references));

        Assert.Equal("new-game.json: startingItems has unknown item id 'megalixir'.", exception.Message);
    }

    [Fact]
    public void Validate_TooManyActiveMembers_ThrowsNamingTheField()
    {
        var newGame = _newGame with { RecruitedMemberIds = ["sarxian", "jazog", "teelle"], ActiveMemberIds = ["sarxian", "jazog", "teelle"] };

        var exception = Assert.Throws<ContentValidationException>(() => NewGameValidator.Validate("new-game.json", newGame, _references));

        Assert.Equal("new-game.json: activeMemberIds must list 1 to 2 members, leader first (was 3).", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidNewGames))]
    public void Validate_InvalidField_ThrowsNamingTheField(NewGameDefinition newGame, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => NewGameValidator.Validate("new-game.json", newGame, _references));

        Assert.Equal($"new-game.json: {problem}", exception.Message);
    }
}
