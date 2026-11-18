using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.NewGame;
using static Jrpg.Editor.Tests.NewGame.NewGameFixtures;

namespace Jrpg.Editor.Tests.NewGame;

// The New game page's boxes (decision 968): written over the record, the typed boxes' problems first, then Data's
// (decision 969); one record, so its host has nothing to choose.
public sealed class NewGameViewModelTests
{
    private readonly NewGameViewModel _newGame = new(World);

    [Fact]
    public void Constructor_NewGame_ShowsGoldStartAndSprite()
    {
        Assert.Equal(
            ("250", "intro", "4.5", "1.5", Facing.Left, "player.png"),
            (_newGame.GoldText, _newGame.MapId, _newGame.XText, _newGame.YText, _newGame.Facing, _newGame.FieldSpriteFile));
    }

    [Fact]
    public void ToNewGame_Untouched_EqualsTheRecord()
    {
        Assert.Equal(TestNewGame, _newGame.ToNewGame());
    }

    [Fact]
    public void ToNewGame_TypedGold_SetsIt()
    {
        _newGame.GoldText = " 1200 ";

        Assert.Equal(1200, _newGame.ToNewGame().StartingGold);
    }

    [Fact]
    public void ToNewGame_TypedStart_SetsMapCellFacing()
    {
        (_newGame.MapId, _newGame.XText, _newGame.YText, _newGame.Facing) = ("forest", "12.25", "3", Facing.Up);

        Assert.Equal(new MapLocation("forest", new Point2D(12.25, 3), Facing.Up), _newGame.ToNewGame().StartLocation);
    }

    [Fact]
    public void ToNewGame_TypedSprite_SetsItTrimmed()
    {
        _newGame.FieldSpriteFile = " npc_placeholder.png ";

        Assert.Equal("npc_placeholder.png", _newGame.ToNewGame().FieldSpriteFile);
    }

    [Fact]
    public void ToNewGame_WithAProblem_Throws()
    {
        _newGame.GoldText = "lots";

        Assert.Throws<InvalidOperationException>(_newGame.ToNewGame);
    }

    [Fact]
    public void Problem_Untouched_IsNull()
    {
        Assert.Null(_newGame.Problem);
    }

    [Fact]
    public void Problem_GoldNotANumber_NamesGold()
    {
        _newGame.GoldText = "lots";

        Assert.Equal("Starting gold must be a whole number (was 'lots').", _newGame.Problem);
    }

    // double.TryParse reads both whatever the styles; the save would throw (.claude/rules/editor.md).
    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void Problem_XNotFinite_NamesX(string typed)
    {
        _newGame.XText = typed;

        Assert.Equal($"Start x must be a number, with a dot for decimals (was '{typed}').", _newGame.Problem);
    }

    [Fact]
    public void Problem_YNotANumber_NamesY()
    {
        _newGame.YText = "1,5";

        Assert.Equal("Start y must be a number, with a dot for decimals (was '1,5').", _newGame.Problem);
    }

    // The intro map is 6 cells wide: 5.99 is in its last column, 6 past it.
    [Fact]
    public void Problem_StartOutsideTheMap_ShowsDataMessage()
    {
        _newGame.XText = "6";

        Assert.Contains("startLocation.position (6, 1.5) must be inside map 'intro' (6x3).", _newGame.Problem);
    }

    [Fact]
    public void Problem_StartInTheMapsLastColumn_IsNull()
    {
        _newGame.XText = "5.99";

        Assert.Null(_newGame.Problem);
    }

    [Fact]
    public void Problem_SpriteWithoutRecord_ShowsDataMessage()
    {
        _newGame.FieldSpriteFile = "Stranger.png";

        Assert.Contains("Stranger.png", _newGame.Problem);
    }

    [Theory]
    [InlineData(nameof(NewGameViewModel.GoldText))]
    [InlineData(nameof(NewGameViewModel.MapId))]
    [InlineData(nameof(NewGameViewModel.XText))]
    [InlineData(nameof(NewGameViewModel.YText))]
    [InlineData(nameof(NewGameViewModel.Facing))]
    [InlineData(nameof(NewGameViewModel.FieldSpriteFile))]
    public void Problem_TypedChange_RaisesProblemChanged(string box)
    {
        var raised = false;
        _newGame.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(NewGameViewModel.Problem);

        Type(box);

        Assert.True(raised);
    }

    // The draft's maps held in reverse order, so a list read in the dictionary's order fails.
    [Fact]
    public void MapIds_DraftMaps_ListsThemById()
    {
        var newGame = new NewGameViewModel(World with { Maps = World.Maps.OrderByDescending(map => map.Key).ToDictionary() });

        Assert.Equal(["forest", "interior_1", "intro", "start", "to_aslas_1"], newGame.MapIds);
    }

    [Fact]
    public void MapIds_DraftLackingTheStartMap_ListsIt()
    {
        var newGame = new NewGameViewModel(
            World with { NewGame = TestNewGame with { StartLocation = TestNewGame.StartLocation with { MapId = "lost" } } });

        Assert.Contains("lost", newGame.MapIds);
    }

    // Held in reverse order, as the maps above.
    [Fact]
    public void FieldSpriteFiles_DraftSprites_ListsThemByFile()
    {
        var newGame = new NewGameViewModel(
            World with { FieldSprites = World.FieldSprites.OrderByDescending(sprite => sprite.Key).ToDictionary() });

        Assert.Equal(["npc_placeholder.png", "player.png"], newGame.FieldSpriteFiles);
    }

    [Fact]
    public void ChosenKey_IsNull()
    {
        Assert.Null(_newGame.ChosenKey);
    }

    [Fact]
    public void Choose_AnyKey_ChangesNothing()
    {
        _newGame.GoldText = "300";

        _newGame.Choose("intro");

        Assert.Equal(TestNewGame with { StartingGold = 300 }, _newGame.ToNewGame());
    }

    private void Type(string box)
    {
        switch (box)
        {
            case nameof(NewGameViewModel.GoldText):
                _newGame.GoldText = "300";
                break;
            case nameof(NewGameViewModel.MapId):
                _newGame.MapId = "forest";
                break;
            case nameof(NewGameViewModel.XText):
                _newGame.XText = "1";
                break;
            case nameof(NewGameViewModel.YText):
                _newGame.YText = "1";
                break;
            case nameof(NewGameViewModel.Facing):
                _newGame.Facing = Facing.Right;
                break;
            default:
                _newGame.FieldSpriteFile = "npc_placeholder.png";
                break;
        }
    }
}
