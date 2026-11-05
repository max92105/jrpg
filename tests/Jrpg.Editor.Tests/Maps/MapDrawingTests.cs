using System.Windows.Media;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Maps;

// An object of one forest tile given 4 frames of 140 ms; the forest sheet has 32 columns, so the frames fit.
public sealed class MapDrawingTests
{
    private static readonly string _root = Path.Combine(AppContext.BaseDirectory, "test-world");

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(_root));
    private readonly TileDefinition _tile;
    private readonly TileImages _images;
    private readonly MapDrawing _drawing;

    public MapDrawingTests()
    {
        _tile = _draft.Current.Tiles.Values.First(tile => tile.SheetId == "fg_forest_fall" && tile.Column + 4 <= 32);
        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [_tile.Id] = new TileAnimation { FrameCount = 4, FrameMilliseconds = 140 } });
        _images = new TileImages(_draft, ArtFolders.RootIn(_root), () => MissingArt.None);
        _drawing = new MapDrawing(_draft, _images);
    }

    [Fact]
    public void DrawObject_AtTheFrameTime_DrawsTheSecondFrame()
    {
        var drawing = (DrawingGroup)_drawing.DrawObject(OneTileObject(_tile.Id), TimeSpan.FromMilliseconds(140)).Drawing;

        var drawn = Assert.Single(drawing.Children.OfType<ImageDrawing>());
        Assert.Same(_images.CellImageOf(new SheetCell(_tile.SheetId, _tile.Column + 1, _tile.Row)), drawn.ImageSource);
    }

    [Fact]
    public void DrawObject_StackedObject_DrawsTheUpperLevelLast()
    {
        var still = _draft.Current.Tiles.Values.Where(tile => tile.Animation is null && tile.SheetId == _tile.SheetId).Take(2).ToList();
        var stacked = OneTileObject(still[0].Id) with { Tiles = [new ObjectTile(0, 0, still[0].Id), new ObjectTile(0, 0, still[1].Id, Level: 1)] };

        var drawing = (DrawingGroup)_drawing.DrawObject(stacked).Drawing;

        var drawn = drawing.Children.OfType<ImageDrawing>().ToList();
        Assert.Equal(2, drawn.Count);
        Assert.Same(_images.CellImageOf(new SheetCell(still[1].SheetId, still[1].Column, still[1].Row)), drawn[1].ImageSource);
    }

    [Fact]
    public void IsAnimated_ObjectWithAnAnimatedTile_IsTrue() => Assert.True(_drawing.IsAnimated(OneTileObject(_tile.Id)));

    [Fact]
    public void IsAnimated_StillObject_IsFalse() =>
        Assert.False(_drawing.IsAnimated(OneTileObject(_draft.Current.Tiles.Values.First(tile => tile.Animation is null).Id)));

    private static ObjectDefinition OneTileObject(int tileId) => new()
    {
        Id = "waterfall",
        Category = "General",
        Width = 1,
        Height = 1,
        Tiles = [new ObjectTile(0, 0, tileId)],
        CollisionPolygon = [],
        AbovePlayerZones = [],
    };
}
