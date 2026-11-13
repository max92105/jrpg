using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

/// <summary>
/// One sprite image's setup (ADR 0043) over the test world's records: the player's 384 x 288 sheet in 48 px frames, and
/// a bought sheet's shape, 128 x 224 in 32 px frames, 4 x 7.
/// </summary>
public sealed class FieldSpriteSetupViewModelTests
{
    private const string Bought = "villager.png";

    private static readonly string _testWorld = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_testWorld);
    private static readonly (int Width, int Height) _boughtSize = (128, 224);
    private static readonly FieldSpriteSheet _player = _catalog.FieldSprites["player.png"];
    private static readonly int[] _mixedWidths = [24, 16, 16];

    private readonly CatalogDraft _draft = new(_catalog);

    [Fact]
    public void ForNew_FourBySevenSquareSheet_GuessesFourColumnsSevenRows()
    {
        var setup = FieldSpriteSetupViewModel.ForNew(Bought, _boughtSize, _draft);

        Assert.Equal((4, 7, 32, 32), (setup.Columns, setup.Rows, setup.FrameWidth, setup.FrameHeight));
    }

    [Fact]
    public void ForNew_HeightNotSplittingSquare_GuessesOneRow()
    {
        var setup = FieldSpriteSetupViewModel.ForNew(Bought, (128, 100), _draft);

        Assert.Equal(1, setup.Rows);
    }

    // Nine of the test world's sheets are 48 px, so the guess is 1 there; a catalog of 16 px sheets gives 3.
    [Fact]
    public void ForNew_ManaSeedSheetsSetUp_GuessesScaleThree()
    {
        var sixteen = _catalog.Tilesheets.Values
            .Select(sheet => sheet with { TileWidth = 16, TileHeight = 16 })
            .ToDictionary(sheet => sheet.Id);

        var manaSeed = new CatalogDraft(_catalog with { Tilesheets = sixteen });

        var setup = FieldSpriteSetupViewModel.ForNew(Bought, _boughtSize, manaSeed);

        Assert.Equal(3, setup.Scale);
    }

    // Two 16 px sheets against one 24 px: the commonest width wins, 48 / 16.
    [Fact]
    public void ForNew_MixedSheetWidths_GuessesFromTheCommonest()
    {
        var sheets = _catalog.Tilesheets.Values.Take(3).ToList();
        var mixed = _mixedWidths
            .Zip(sheets, (width, sheet) => sheet with { TileWidth = width, TileHeight = width })
            .ToDictionary(sheet => sheet.Id);
        var draft = new CatalogDraft(_catalog with { Tilesheets = mixed });

        var setup = FieldSpriteSetupViewModel.ForNew(Bought, _boughtSize, draft);

        Assert.Equal(3, setup.Scale);
    }

    [Fact]
    public void ForNew_NoSheetSetUp_GuessesScaleOne()
    {
        var none = new CatalogDraft(_catalog with { Tilesheets = new Dictionary<string, TilesheetDefinition>() });

        Assert.Equal(1, FieldSpriteSetupViewModel.ForNew(Bought, _boughtSize, none).Scale);
    }

    [Fact]
    public void ForNew_Defaults_WalkRowsDownLeftRightUpOverFourFrames()
    {
        var walk = FieldSpriteSetupViewModel.ForNew(Bought, _boughtSize, _draft).Walk;

        Assert.Equal((0, 1, 2, 3), (walk.DownRow, walk.LeftRow, walk.RightRow, walk.UpRow));
        Assert.Equal((4, FieldSpriteSetupViewModel.NewFrameMilliseconds), (walk.Frames, walk.FrameMilliseconds));
    }

    [Fact]
    public void From_PlayerRecord_ReadsItsNumbers()
    {
        var setup = PlayerSetup();

        var walk = setup.Walk;
        Assert.Equal((8, 6, 48, 48), (setup.Columns, setup.Rows, setup.FrameWidth, setup.FrameHeight));
        Assert.Equal((1, 0), (setup.Scale, setup.FeetPixels));
        Assert.Equal((0, 1, 2, 3), (walk.DownRow, walk.LeftRow, walk.RightRow, walk.UpRow));
        Assert.Equal((8, 100), (walk.Frames, walk.FrameMilliseconds));
    }

    [Fact]
    public void ToSheet_PlayerRecordUnchanged_EqualsTheRecord() => Assert.Equal(_player, PlayerSetup().ToSheet());

    [Fact]
    public void Problem_ColumnsNotSplittingWidth_IsGridCutMessage()
    {
        var setup = PlayerSetup();

        setup.Columns = 7;

        Assert.Equal("384 px doesn't split into 7 columns.", setup.Problem);
    }

    [Fact]
    public void Problem_FacingRowPastRows_NamesTheFacing()
    {
        var setup = PlayerSetup();

        setup.Walk.UpRow = 6;

        Assert.Equal("The up row must be within 1..6 (was 7).", setup.Problem);
    }

    [Fact]
    public void Problem_WalkFramesPastColumns_SaysSo()
    {
        var setup = PlayerSetup();

        setup.Walk.Frames = 9;

        Assert.Equal("The walk's 9 frames don't fit in 8 columns.", setup.Problem);
    }

    [Fact]
    public void Problem_ScaleZero_IsDataMessage()
    {
        var setup = PlayerSetup();

        setup.Scale = 0;

        Assert.Equal("field-sprites.json: field sprite 'player.png': scale must be within 1..8 (was 0).", setup.Problem);
    }

    [Fact]
    public void Problem_ChangingRowsBackIntoRange_Clears()
    {
        var setup = PlayerSetup();
        setup.Rows = 3;
        Assert.NotNull(setup.Problem);

        setup.Rows = 6;

        Assert.Null(setup.Problem);
    }

    [Fact]
    public void Problem_WalkRowEditedInsideTheCycle_RaisesTheChange()
    {
        var setup = PlayerSetup();
        var raised = new List<string?>();
        setup.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        setup.Walk.LeftRow = 9;

        Assert.Contains(nameof(FieldSpriteSetupViewModel.Problem), raised);
        Assert.NotNull(setup.Problem);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 1)]
    [InlineData(750, 7)]
    public void PreviewCellAt_WalkingLeft_StepsAlongTheLeftRow(int milliseconds, int column)
    {
        var walked = TimeSpan.FromMilliseconds(milliseconds);

        Assert.Equal(new SpriteCell(column, 1), PlayerSetup().PreviewCellAt(Facing.Left, walked));
    }

    [Fact]
    public void PreviewCellAt_WithProblem_IsNull()
    {
        var setup = PlayerSetup();

        setup.Scale = 0;

        Assert.Null(setup.PreviewCellAt(Facing.Down, TimeSpan.Zero));
    }

    [Fact]
    public void RowChoices_RowsChanged_Follows()
    {
        var setup = PlayerSetup();

        setup.Rows = 2;

        Assert.Equal([0, 1], setup.RowChoices);
    }

    private FieldSpriteSetupViewModel PlayerSetup() => FieldSpriteSetupViewModel.From(_player, (384, 288), _draft);
}
