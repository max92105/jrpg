using Jrpg.Core.Progression;
using Jrpg.Editor.Catalogs;
using static Jrpg.Editor.Tests.Progression.ExperienceFixtures;

namespace Jrpg.Editor.Tests.Catalogs;

// The experience table is replaced whole as one undo step (decision 955), compared by its amounts in order, so a page
// applied unchanged records nothing; the rest of the rules stays as it is.
public sealed class CatalogDraftExperienceTests
{
    private readonly CatalogDraft _draft = new(World);

    [Fact]
    public void SetExperienceTable_ChangedRow_RecordsOneStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _draft.SetExperienceTable(new ExperienceTable([10, 25, 40]));

        Assert.Equal(1, steps);
        Assert.Equal([10, 25, 40], _draft.Current.Rules.Experience.ExperienceToNextLevel);
    }

    // A new table of the same amounts, as the page builds it: equal by content, not by reference.
    [Fact]
    public void SetExperienceTable_SameList_RecordsNothing()
    {
        _draft.SetExperienceTable(new ExperienceTable([10, 20, 40]));

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetExperienceTable_Undo_RestoresTheTable()
    {
        _draft.SetExperienceTable(new ExperienceTable([10, 25, 40]));

        _draft.Undo();

        Assert.Same(TestTable, _draft.Current.Rules.Experience);
    }

    [Fact]
    public void SetExperienceTable_KeepsTheOtherRules()
    {
        var table = new ExperienceTable([10, 25, 40]);

        _draft.SetExperienceTable(table);

        Assert.Equal(World.Rules with { Experience = table }, _draft.Current.Rules);
    }
}
