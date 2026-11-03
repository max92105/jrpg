using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

public sealed class UndoJournalTests
{
    private readonly UndoJournal _journal = new();

    [Fact]
    public void Undo_MapStepThenCatalogStep_UndoesTheCatalogFirst()
    {
        _journal.Record(UndoTarget.Map);
        _journal.Record(UndoTarget.Catalogs);

        Assert.Equal([UndoTarget.Catalogs, UndoTarget.Map], [_journal.Undo(), _journal.Undo()]);
    }

    // A catalog redo (a removal) could take what the closed map, saved since, now uses; nothing could check it.
    [Fact]
    public void ForgetMapSteps_UndoneCatalogStep_CanNoLongerBeRedone()
    {
        _journal.Record(UndoTarget.Catalogs);
        _journal.Undo();

        _journal.ForgetMapSteps();

        Assert.False(_journal.CanRedo);
    }

    [Fact]
    public void Undo_AfterAnotherMapOpened_SkipsTheClosedMapsSteps()
    {
        _journal.Record(UndoTarget.Catalogs);
        _journal.Record(UndoTarget.Map);

        _journal.ForgetMapSteps();

        Assert.Equal(UndoTarget.Catalogs, _journal.Undo());
        Assert.False(_journal.CanUndo);
    }

    [Fact]
    public void ForgetUndoneCatalogSteps_MapStepUndoneToo_KeepsTheMapStep()
    {
        _journal.Record(UndoTarget.Map);
        _journal.Record(UndoTarget.Catalogs);
        _journal.Undo();
        _journal.Undo();

        _journal.ForgetUndoneCatalogSteps();

        Assert.Equal(UndoTarget.Map, _journal.Redo());
        Assert.False(_journal.CanRedo);
    }

    [Fact]
    public void Redo_AfterANewStep_HasNothing()
    {
        _journal.Record(UndoTarget.Catalogs);
        _journal.Undo();

        _journal.Record(UndoTarget.Map);

        Assert.False(_journal.CanRedo);
    }
}
