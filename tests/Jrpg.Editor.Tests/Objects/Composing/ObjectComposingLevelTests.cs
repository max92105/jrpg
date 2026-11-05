using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Tests.Shell;

namespace Jrpg.Editor.Tests.Objects.Composing;

/// <summary>New object and Edit object with a stack of levels (ADR 0040), on a copy of the test world.</summary>
public sealed class ObjectComposingLevelTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    // A ground tile as the wall on the base, a city tile as the window over it on level 1.
    [Fact]
    public void TryApply_OverlayOnLevelOneOverABaseCell_SavesBothTilesBaseFirst()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var composer = viewModel.Composing!.StartNew();
        (composer.Naming.Id, composer.Naming.Category) = ("house_window", "House");
        var wall = PaintTileOf(viewModel, composer, "ground", level: 0);
        var overlay = PaintTileOf(viewModel, composer, "city", level: 1);

        Assert.True(viewModel.Composing.TryApply(composer));

        Assert.Equal([new ObjectTile(0, 0, wall), new ObjectTile(0, 0, overlay, Level: 1)], viewModel.Catalog!.Objects["house_window"].Tiles);
    }

    // A ground tile as the base, a city tile on -1 under it on the same cell, painted base first.
    [Fact]
    public void TryApply_TileUnderTheBase_SavesItFirst()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var composer = viewModel.Composing!.StartNew();
        (composer.Naming.Id, composer.Naming.Category) = ("house_shadow", "House");
        var ground = PaintTileOf(viewModel, composer, "ground", level: 0);
        var shadow = PaintTileOf(viewModel, composer, "city", level: -1);

        Assert.True(viewModel.Composing.TryApply(composer));

        Assert.Equal([new ObjectTile(0, 0, shadow, Level: -1), new ObjectTile(0, 0, ground)], viewModel.Catalog!.Objects["house_shadow"].Tiles);
    }

    [Fact]
    public void StartEdit_StackedObject_ChoosesTheBaseWithTheUpperTileOnItsLevel()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var chest = viewModel.Catalog!.Objects["chest"];
        var baseTile = chest.Tiles[0];
        viewModel.Content!.SetObject(chest with { Tiles = [.. chest.Tiles, baseTile with { Level = 2 }] });
        var browser = viewModel.Placing!.Browser;
        browser.SelectedObject = browser.Objects.First(definition => definition.Id == "chest");

        var painting = viewModel.Composing!.StartEdit()!.Painting;

        Assert.Equal(0, painting.Level);
        Assert.True(painting.Canvas.Cells.ContainsKey(new CellOnLevel(new GridCell(baseTile.X, baseTile.Y), 2)));
    }

    // Chooses the sheet's first tile in the Tiles panel and strokes it on the canvas's top-left cell on the level.
    private static int PaintTileOf(MainViewModel viewModel, ObjectComposerViewModel composer, string sheetId, int level)
    {
        var tileId = viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId == sheetId).Id;
        viewModel.Painting!.Browser.Select(tileId);
        composer.Painting.Level = level;
        composer.Painting.BeginStroke();
        composer.Painting.PaintAt(new GridCell(0, 0));
        composer.Painting.EndStroke();
        return tileId;
    }
}
