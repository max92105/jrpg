using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// The Items page's icon note (decisions 879, 442): the chosen item's icon when the icons folder lacks it, read again
// when another item is chosen or its icon box changes.
public sealed class ItemCatalogViewModelIconTests
{
    private readonly ItemCatalogViewModel _catalog = new(ItemFixtures.Catalog, ["Ore.png", "Vest.png"]);

    [Fact]
    public void MissingIconNote_IconNotInTheFolder_NamesIt()
    {
        _catalog.Choose("tonic");

        Assert.Equal("Tonic.png isn't in art/icons: the game will refuse to start until it is.", _catalog.MissingIconNote);
    }

    [Fact]
    public void MissingIconNote_IconBoxEmpty_AsksForOne()
    {
        _catalog.Choose("tonic");

        _catalog.SelectedItem!.IconFile = "";

        Assert.Equal("No icon: the game will refuse to start until one of art/icons is named.", _catalog.MissingIconNote);
    }

    [Fact]
    public void MissingIconNote_IconTypedThatExists_IsCleared()
    {
        _catalog.Choose("tonic");
        Assert.NotEmpty(_catalog.MissingIconNote);

        _catalog.SelectedItem!.IconFile = "Ore.png";

        Assert.Empty(_catalog.MissingIconNote);
    }

    // Windows finds the file whatever the case of its name, so the game's start takes it.
    [Fact]
    public void MissingIconNote_IconTypedInAnotherCase_IsEmpty()
    {
        _catalog.Choose("tonic");

        _catalog.SelectedItem!.IconFile = "ore.PNG";

        Assert.Empty(_catalog.MissingIconNote);
    }

    [Fact]
    public void MissingIconNote_OtherItemChosen_IsRecomputed()
    {
        _catalog.Choose("tonic");
        Assert.NotEmpty(_catalog.MissingIconNote);

        _catalog.Choose("copper_vest");

        Assert.Empty(_catalog.MissingIconNote);
    }

    [Fact]
    public void MissingIconNote_IconTyped_IsRaised()
    {
        _catalog.Choose("tonic");
        var raised = new List<string?>();
        _catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.SelectedItem!.IconFile = "Ore.png";

        Assert.Contains(nameof(ItemCatalogViewModel.MissingIconNote), raised);
    }

    [Fact]
    public void MissingIconNote_OtherItemChosen_IsRaised()
    {
        var raised = new List<string?>();
        _catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.Choose("ore");

        Assert.Contains(nameof(ItemCatalogViewModel.MissingIconNote), raised);
    }
}
