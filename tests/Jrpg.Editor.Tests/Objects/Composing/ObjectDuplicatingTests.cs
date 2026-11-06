using Jrpg.Core.Maps;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Tests.Shell;

namespace Jrpg.Editor.Tests.Objects.Composing;

/// <summary>Objects > Duplicate object through the editor (decision 517), on a copy of the test world.</summary>
public sealed class ObjectDuplicatingTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void StartDuplicate_TileObjectChosen_StartsWithItsCellsAndCategoryAndNoId()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var chest = ChooseObject(viewModel, "chest");

        var composer = viewModel.Composing!.StartDuplicate()!;

        Assert.Same(chest, composer.ShapeSource);
        Assert.Null(composer.Original);
        Assert.Equal(("", chest.Category, true), (composer.Naming.Id, composer.Naming.Category, composer.IsIdEditable));
        Assert.Equal(ObjectCanvas.Of(chest, viewModel.Catalog!.Tiles).Cells, composer.Painting.Canvas.Cells);
    }

    [Fact]
    public void StartDuplicate_NpcChosen_IsNull()
    {
        var viewModel = _content.Open(_content.TestWorld);

        ChooseObject(viewModel, "villager");

        Assert.Null(viewModel.Composing!.StartDuplicate());
    }

    [Fact]
    public void TryApply_DuplicateNamed_MakesANewObjectWithTheCopiedShapesAndKeepsTheOriginal()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var chest = ChooseObject(viewModel, "chest");
        var composer = viewModel.Composing!.StartDuplicate()!;
        composer.Naming.Id = "chest_2";

        Assert.True(viewModel.Composing.TryApply(composer));

        Assert.Same(chest, viewModel.Catalog!.Objects["chest"]);
        var duplicate = viewModel.Catalog.Objects["chest_2"];
        Assert.Same(duplicate, viewModel.Placing!.Browser.SelectedObject);
        Assert.Equal((chest.Category, chest.Width, chest.Height), (duplicate.Category, duplicate.Width, duplicate.Height));
        Assert.Equal(chest.Tiles, duplicate.Tiles);
        Assert.Equal(chest.CollisionPolygon, duplicate.CollisionPolygon);
        Assert.Equal(chest.AbovePlayerZones, duplicate.AbovePlayerZones);
    }

    [Fact]
    public void TryApply_DuplicateWithTheCopiedObjectsId_ReturnsFalse()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        ChooseObject(viewModel, "chest");
        var composer = viewModel.Composing!.StartDuplicate()!;
        composer.Naming.Id = "chest";

        Assert.False(viewModel.Composing.TryApply(composer));
    }

    [Fact]
    public void TryApply_DuplicateUndo_RemovesTheDuplicateAsOneStep()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        ChooseObject(viewModel, "chest");
        var composer = viewModel.Composing!.StartDuplicate()!;
        composer.Naming.Id = "chest_2";
        viewModel.Composing.TryApply(composer);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.False(viewModel.Catalog!.Objects.ContainsKey("chest_2"));
    }

    private static ObjectDefinition ChooseObject(MainViewModel viewModel, string objectId)
    {
        var browser = viewModel.Placing!.Browser;
        browser.SelectedObject = browser.Objects.First(definition => definition.Id == objectId);
        return browser.SelectedObject;
    }
}
