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
