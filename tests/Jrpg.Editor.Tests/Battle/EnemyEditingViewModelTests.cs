using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Battle;

// The Enemies window starts from the draft and its whole session is one undo step (decision 591).
public sealed class EnemyEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly EnemyEditingViewModel _enemyEditing;

    public EnemyEditingViewModelTests() => _enemyEditing = new EnemyEditingViewModel(_draft, _ => []);

    [Fact]
    public void Start_Draft_ListsItsEnemies()
    {
        var edited = _enemyEditing.Start();

        Assert.Contains(edited.Enemies, enemy => enemy.Id == "goblin_chief" && !enemy.IsNew);
    }

    [Fact]
    public void Apply_DuplicatedEnemy_IsOneUndoStep()
    {
        var edited = _enemyEditing.Start();
        edited.SelectedEnemy = edited.Enemies.Single(enemy => enemy.Id == "goblin_chief");
        edited.DuplicateEnemyCommand.Execute(null);
        edited.SelectedEnemy!.Id = "wolf_chief";
        edited.SelectedEnemy.Name = "Wolf Chief";
        edited.SelectedEnemy.MaxHpText = "300";

        _enemyEditing.Apply(edited);

        var wolfChief = _draft.Current.Enemies["wolf_chief"];
        Assert.Equal(("Wolf Chief", 300), (wolfChief.Name, wolfChief.MaxHp));
        Assert.Equal(_draft.Current.Enemies["goblin_chief"].Tactics, wolfChief.Tactics);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
        Assert.False(_draft.Current.Enemies.ContainsKey("wolf_chief"));
    }

    [Fact]
    public void Apply_Unchanged_RecordsNothing()
    {
        _enemyEditing.Apply(_enemyEditing.Start());

        Assert.False(_draft.CanUndo);
    }

    // The window reads the draft when it opens, so a table added before is offered, after the saved ones by id.
    [Fact]
    public void Start_AfterSetDropTables_OffersTheNewTable()
    {
        _draft.SetDropTables([.. _draft.Current.DropTables.Values, new DropTable("zz_slime", [new DropEntry("potion", 50)])]);

        var choices = _enemyEditing.Start().DropTableChoices;

        Assert.Equal("", choices[0]);
        Assert.Equal("zz_slime", choices[^1]);
        Assert.Contains("goblin", choices);
    }
}
