using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

public sealed class UndoJournalTests
{
    private readonly UndoJournal _journal = new();

    [Fact]
    public void Undo_MapStepThenCatalogStep_UndoesTheCatalogFirst()
    {
        _journal.Record(UndoStep.OnMap("a"));
        _journal.Record(UndoStep.Catalogs);

        Assert.Equal([UndoStep.Catalogs, UndoStep.OnMap("a")], [_journal.Undo("a"), _journal.Undo("a")]);
    }

    [Fact]
    public void Undo_StepsOnTwoMaps_UndoesOnlyTheShownMapsAndTheCatalogs()
    {
        _journal.Record(UndoStep.OnMap("a"));
        _journal.Record(UndoStep.Catalogs);
        _journal.Record(UndoStep.OnMap("b"));

        Assert.Equal([UndoStep.Catalogs, UndoStep.OnMap("a")], [_journal.Undo("a"), _journal.Undo("a")]);
        Assert.False(_journal.CanUndo("a"));
    }

    [Fact]
    public void Undo_OtherMapShownAgain_ItsStepsAreStillThere()
    {
        _journal.Record(UndoStep.OnMap("b"));
        _journal.Record(UndoStep.OnMap("a"));
        _journal.Undo("a");

        Assert.Equal(UndoStep.OnMap("b"), _journal.Undo("b"));
    }

    [Fact]
    public void CanUndo_OnlyAnotherMapsSteps_IsFalse()
    {
        _journal.Record(UndoStep.OnMap("b"));

        Assert.False(_journal.CanUndo("a"));
        Assert.Null(_journal.NextUndo("a"));
    }

    [Fact]
    public void Redo_UndoneOnMapA_RedoesOnlyWhenAIsShown()
    {
        _journal.Record(UndoStep.OnMap("a"));
        _journal.Undo("a");

        Assert.False(_journal.CanRedo("b"));
        Assert.Equal(UndoStep.OnMap("a"), _journal.Redo("a"));
    }

    [Fact]
    public void ForgetUndoneCatalogSteps_MapStepUndoneToo_KeepsTheMapStep()
    {
        _journal.Record(UndoStep.OnMap("a"));
        _journal.Record(UndoStep.Catalogs);
        _journal.Undo("a");
        _journal.Undo("a");

        _journal.ForgetUndoneCatalogSteps();

        Assert.Equal(UndoStep.OnMap("a"), _journal.Redo("a"));
        Assert.False(_journal.CanRedo("a"));
    }

    // Map a's undone step is newer than the catalog step: a may use, as it is now, what the catalog redo removes.
    [Fact]
    public void CanRedo_AnotherMapsUndoneStepNewerThanACatalogStep_IsFalse()
    {
        _journal.Record(UndoStep.OnMap("a"));
        _journal.Record(UndoStep.Catalogs);
        _journal.Undo("a");
        _journal.Undo("a");

        Assert.False(_journal.CanRedo("b"));
        Assert.Equal(UndoStep.OnMap("a"), _journal.Redo("a"));
    }

    [Fact]
    public void Redo_CatalogStepNewerThanAnotherMapsStep_RedoesIt()
    {
        _journal.Record(UndoStep.Catalogs);
        _journal.Record(UndoStep.OnMap("a"));
        _journal.Undo("a");
        _journal.Undo("a");

        Assert.Equal(UndoStep.Catalogs, _journal.Redo("b"));
    }

    [Fact]
    public void Redo_AnotherMapsStepNewerThanTheShownMaps_SkipsIt()
    {
        _journal.Record(UndoStep.OnMap("b"));
        _journal.Record(UndoStep.OnMap("a"));
        _journal.Undo("a");
        _journal.Undo("b");

        Assert.Equal(UndoStep.OnMap("a"), _journal.Redo("a"));
    }

    // A new edit could start using what a redo would take away, on any map.
    [Fact]
    public void Redo_AfterANewStepOnAnotherMap_HasNothing()
    {
        _journal.Record(UndoStep.Catalogs);
        _journal.Undo("a");

        _journal.Record(UndoStep.OnMap("b"));

        Assert.False(_journal.CanRedo("a"));
    }
}
