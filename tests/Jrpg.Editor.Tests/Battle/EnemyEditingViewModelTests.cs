using Jrpg.Core.Battle;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Battle;

// The Enemies page starts from the draft and its whole session is one undo step (decision 591).
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

    // Every rule is rebuilt from its rows into new lists, which the draft compares by content, so an untouched session
    // still records no step.
    [Fact]
    public void Apply_UntouchedSession_RecordsNothing()
    {
        var edited = _enemyEditing.Start();
        var rebuilt = edited.ToEnemies().Single(enemy => enemy.Id == "goblin_chief");
        Assert.NotSame(_draft.Current.Enemies["goblin_chief"].Tactics, rebuilt.Tactics);

        _enemyEditing.Apply(edited);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void Apply_RuleTargetChanged_WritesIt()
    {
        var (editing, draft) = WithGoblinAttacking();
        var edited = editing.Start();

        GoblinsRule(edited).On = TacticTargetChoice.Of(TacticTarget.LowestHpFoe);
        editing.Apply(edited);

        Assert.Equal(TacticTarget.LowestHpFoe, Assert.Single(draft.Current.Enemies["goblin"].Tactics).On);
    }

    [Fact]
    public void Apply_RuleTargetChanged_IsOneUndoStep()
    {
        var (editing, draft) = WithGoblinAttacking();
        var edited = editing.Start();
        GoblinsRule(edited).On = TacticTargetChoice.Of(TacticTarget.LowestHpFoe);

        editing.Apply(edited);
        draft.Undo();

        Assert.False(draft.CanUndo);
        Assert.Null(Assert.Single(draft.Current.Enemies["goblin"].Tactics).On);
    }

    // The page reads the draft when it opens, so a table added before is offered, after the saved ones by id.
    [Fact]
    public void Start_AfterSetDropTables_OffersTheNewTable()
    {
        _draft.SetDropTables([.. _draft.Current.DropTables.Values, new DropTable("zz_slime", [new DropEntry("potion", 50)])]);

        var choices = _enemyEditing.Start().DropTableChoices;

        Assert.Equal("", choices[0]);
        Assert.Equal("zz_slime", choices[^1]);
        Assert.Contains("goblin", choices);
    }

    // The test world with the goblin's rules the test's own: one attack, aimed automatically.
    private static (EnemyEditingViewModel Editing, CatalogDraft Draft) WithGoblinAttacking()
    {
        var world = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
        var goblin = world.Enemies["goblin"] with { Tactics = [new EnemyAction { Weight = 4 }], Phases = [] };
        var enemies = new Dictionary<string, EnemyDefinition>(world.Enemies) { ["goblin"] = goblin };
        var draft = new CatalogDraft(world with { Enemies = enemies });
        return (new EnemyEditingViewModel(draft, _ => []), draft);
    }

    private static TacticRuleRow GoblinsRule(EnemyCatalogViewModel edited) =>
        edited.Enemies.Single(enemy => enemy.Id == "goblin").Tactics.Lists[0].Rules[0];
}
