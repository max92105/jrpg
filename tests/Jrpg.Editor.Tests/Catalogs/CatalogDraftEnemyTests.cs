using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The enemies are replaced whole as one undo step (decision 591), compared by content so a window reopened and closed
// unchanged records nothing, tactics and phases included.
public sealed class CatalogDraftEnemyTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void SetEnemies_ChangedEnemy_IsOneUndoStep()
    {
        var goblin = _draft.Current.Enemies["goblin"];
        var tougher = goblin with { MaxHp = 99 };

        _draft.SetEnemies([.. _draft.Current.Enemies.Values.Select(enemy => enemy.Id == "goblin" ? tougher : enemy)]);

        Assert.Equal(99, _draft.Current.Enemies["goblin"].MaxHp);
        Assert.True(_draft.IsDirty);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
        Assert.Same(goblin, _draft.Current.Enemies["goblin"]);
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

        _draft.SetEnemies(copies);

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

        _draft.SetEnemies([.. _draft.Current.Enemies.Values.Select(enemy => enemy.Id == "goblin_chief" ? retuned : enemy)]);

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetEnemies_TacticWeightChanged_IsOneUndoStep()
    {
        var chief = _draft.Current.Enemies["goblin_chief"];
        var retuned = chief with { Tactics = [.. chief.Tactics.Select(action => action with { Weight = action.Weight + 1 })] };

        _draft.SetEnemies([.. _draft.Current.Enemies.Values.Select(enemy => enemy.Id == "goblin_chief" ? retuned : enemy)]);

        Assert.True(_draft.CanUndo);
    }
}
