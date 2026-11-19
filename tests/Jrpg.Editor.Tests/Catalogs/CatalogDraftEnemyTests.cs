using Jrpg.Core.Battle;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The enemies are replaced whole as one undo step (decision 591), compared by content so a window reopened and closed
// unchanged records nothing, tactics and phases included.
public sealed class CatalogDraftEnemyTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    private readonly Dictionary<string, string> _noRenames = [];

    [Fact]
    public void SetEnemies_ChangedEnemy_IsOneUndoStep()
    {
        var goblin = _draft.Current.Enemies["gnoll"];
        var tougher = goblin with { MaxHp = 99 };

        ReplaceEnemy(tougher);

        Assert.Equal(99, _draft.Current.Enemies["gnoll"].MaxHp);
        Assert.True(_draft.IsDirty);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
        Assert.Same(goblin, _draft.Current.Enemies["gnoll"]);
    }

    [Fact]
    public void SetEnemies_EqualByContent_RecordsNothing()
    {
        var copies = _draft.Current.Enemies.Values
            .Select(enemy => enemy with
            {
                Tactics = [.. enemy.Tactics],
                Phases = [.. enemy.Phases.Select(phase => phase with { Tactics = [.. phase.Tactics] })],
            })
            .ToList();

        _draft.SetEnemies(copies, _noRenames);

        Assert.False(_draft.CanUndo);
    }

    // A change inside a phase's tactics counts too: the phases are compared one by one, their tactics by content.
    [Fact]
    public void SetEnemies_PhaseTacticWeightChanged_IsOneUndoStep()
    {
        var chief = _draft.Current.Enemies["goblin_chief"];
        var phase = chief.Phases[0];
        var heavier = phase.Tactics[0] with { Weight = phase.Tactics[0].Weight + 1 };
        var retuned = chief with { Phases = [phase with { Tactics = [heavier, .. phase.Tactics.Skip(1)] }] };

        ReplaceEnemy(retuned);

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetEnemies_TacticWeightChanged_IsOneUndoStep()
    {
        var chief = _draft.Current.Enemies["goblin_chief"];
        var retuned = chief with { Tactics = [.. chief.Tactics.Select(action => action with { Weight = action.Weight + 1 })] };

        ReplaceEnemy(retuned);

        Assert.True(_draft.CanUndo);
    }

    // A swap shows the ids are replaced together, not one after the other.
    [Fact]
    public void SetEnemies_TwoIdsSwapped_EncountersNameTheNewIds()
    {
        var goblin = _draft.Current.Enemies["gnoll"];
        var chief = _draft.Current.Enemies["goblin_chief"];
        var slots = _draft.Current.Encounters["goblin_chief"].EnemyIds;
        var swapped = new Dictionary<string, string> { ["gnoll"] = "goblin_chief", ["goblin_chief"] = "gnoll" };

        _draft.SetEnemies(
            [
                .. _draft.Current.Enemies.Values.Where(enemy => enemy != goblin && enemy != chief),
                goblin with { Id = "goblin_chief" },
                chief with { Id = "gnoll" },
            ],
            swapped);

        var expected = slots.Select(id => swapped.GetValueOrDefault(id, id));
        Assert.Equal(expected, _draft.Current.Encounters["goblin_chief"].EnemyIds);
        Assert.Equal(chief.Name, _draft.Current.Enemies["gnoll"].Name);
    }

    [Fact]
    public void SetEnemies_IdRenamed_UndoBringsBackTheEnemyAndItsEncounters()
    {
        var enemies = _draft.Current.Enemies;
        var encounters = _draft.Current.Encounters;
        var goblin = enemies["gnoll"];

        _draft.SetEnemies(
            [.. enemies.Values.Where(enemy => enemy != goblin), goblin with { Id = "imp" }],
            new Dictionary<string, string> { ["gnoll"] = "imp" });
        _draft.Undo();

        Assert.False(_draft.CanUndo);
        Assert.Same(enemies, _draft.Current.Enemies);
        Assert.Same(encounters, _draft.Current.Encounters);
    }

    private void ReplaceEnemy(EnemyDefinition changed) =>
        _draft.SetEnemies(
            [.. _draft.Current.Enemies.Values.Select(enemy => enemy.Id == changed.Id ? changed : enemy)],
            _noRenames);
}
