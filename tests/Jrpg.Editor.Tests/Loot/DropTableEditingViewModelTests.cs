using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// The Drop tables page starts from the draft and its whole session is one undo step (decision 591).
public sealed class DropTableEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly DropTableEditingViewModel _editing;

    public DropTableEditingViewModelTests() => _editing = new DropTableEditingViewModel(_draft);

    [Fact]
    public void Start_Draft_ListsItsTables()
    {
        var edited = _editing.Start();

        Assert.Contains(edited.Tables, table => table.Id == "goblin" && !table.IsNew);
    }

    [Fact]
    public void Apply_AddedTable_IsOneUndoStep()
    {
        _editing.Apply(WithSlimeTable());

        Assert.Equal(new DropTable("slime_drops", [new DropEntry("potion", 10)]), _draft.Current.DropTables["slime_drops"]);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void Apply_AddedTable_UndoRemovesIt()
    {
        _editing.Apply(WithSlimeTable());

        _draft.Undo();

        Assert.False(_draft.Current.DropTables.ContainsKey("slime_drops"));
    }

    [Fact]
    public void Apply_Unchanged_RecordsNothing()
    {
        _editing.Apply(_editing.Start());

        Assert.False(_draft.CanUndo);
    }

    // A new table, slime_drops, with one potion entry at the starting chance.
    private DropTableCatalogViewModel WithSlimeTable()
    {
        var edited = _editing.Start();
        edited.AddTableCommand.Execute(null);
        var table = edited.SelectedTable!;
        table.Id = "slime_drops";
        table.ItemToAdd = edited.OfferedItems.Single(item => item.Id == "potion");
        table.AddEntryCommand.Execute(null);
        return edited;
    }
}
