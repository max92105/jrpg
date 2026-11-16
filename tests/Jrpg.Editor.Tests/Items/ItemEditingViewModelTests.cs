using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// The Items page starts from the draft and its whole session is one undo step (decision 877).
public sealed class ItemEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(ItemFixtures.Catalog);
    private readonly ItemEditingViewModel _itemEditing;

    public ItemEditingViewModelTests() => _itemEditing = new ItemEditingViewModel(_draft);

    [Fact]
    public void Apply_NameChanged_IsOneUndoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _itemEditing.Apply(TonicRenamed());

        Assert.Equal(1, steps);
        Assert.Equal("Elixir", _draft.Current.Items["tonic"].Name);
    }

    // Every record is rebuilt from its boxes, so only a comparison by content keeps the step out.
    [Fact]
    public void Apply_NothingChanged_RecordsNoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _itemEditing.Apply(_itemEditing.Start());

        Assert.Equal(0, steps);
    }

    [Fact]
    public void Apply_Undo_RestoresTheItems()
    {
        _itemEditing.Apply(TonicRenamed());

        _draft.Undo();

        Assert.Same(ItemFixtures.Tonic, _draft.Current.Items["tonic"]);
    }

    private ItemCatalogViewModel TonicRenamed()
    {
        var edited = _itemEditing.Start();
        edited.Items.Single(item => item.Id == "tonic").Name = "Elixir";
        return edited;
    }
}
