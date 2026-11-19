using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The drop tables are replaced whole as one undo step (decision 591), compared by content; no undo refusal, since
// catalog steps undo in order (decision 599).
public sealed class CatalogDraftDropTableTests
{
    private static readonly DropTable _slime = new("slime", [new DropEntry("potion", 50)]);

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void SetDropTables_ChangedChance_IsOneUndoStep()
    {
        var chance = _draft.Current.DropTables["goblin"].Entries[0].Chance;

        _draft.SetDropTables([.. _draft.Current.DropTables.Values.Select(WithFirstChanceRaised)]);

        Assert.Equal(chance + 1, _draft.Current.DropTables["goblin"].Entries[0].Chance);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetDropTables_Undo_RestoresTheTable()
    {
        var goblin = _draft.Current.DropTables["goblin"];
        _draft.SetDropTables([.. _draft.Current.DropTables.Values, _slime]);

        _draft.Undo();

        Assert.False(_draft.Current.DropTables.ContainsKey("slime"));
        Assert.Same(goblin, _draft.Current.DropTables["goblin"]);
        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetDropTables_EqualByContent_RecordsNothing()
    {
        var copies = _draft.Current.DropTables.Values.Select(table => table with { Entries = [.. table.Entries] }).ToList();

        _draft.SetDropTables(copies);

        Assert.False(_draft.CanUndo);
    }

    // The enemy's pick came after the table was added, so one undo takes the pick back and leaves the table.
    [Fact]
    public void Undo_AfterEnemyTookAddedTable_UndoesTheEnemyStepFirst()
    {
        _draft.SetDropTables([.. _draft.Current.DropTables.Values, _slime]);
        var goblin = _draft.Current.Enemies["gnoll"];
        var takesSlime = goblin with { DropTableId = "slime" };
        _draft.SetEnemies(
            [.. _draft.Current.Enemies.Values.Select(enemy => enemy.Id == "gnoll" ? takesSlime : enemy)],
            new Dictionary<string, string>());

        _draft.Undo();

        Assert.True(_draft.Current.DropTables.ContainsKey("slime"));
        Assert.Equal(goblin.DropTableId, _draft.Current.Enemies["gnoll"].DropTableId);
    }

    private static DropTable WithFirstChanceRaised(DropTable table) =>
        table with { Entries = [table.Entries[0] with { Chance = table.Entries[0].Chance + 1 }, .. table.Entries.Skip(1)] };
}
