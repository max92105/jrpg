using Jrpg.Editor.Catalogs;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Catalogs;

// The classes are replaced whole as one undo step (decision 945), compared by content so a page applied unchanged
// records nothing, the skills and resources included.
public sealed class CatalogDraftClassTests
{
    private readonly CatalogDraft _draft = new(World);

    [Fact]
    public void SetClasses_ChangedGrowth_RecordsOneStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _draft.SetClasses([Archer, Scholar with { Growth = Scholar.Growth with { Speed = 5 } }]);

        Assert.Equal(1, steps);
        Assert.Equal(5, _draft.Current.Classes["scholar"].Growth.Speed);
    }

    // Every record is rebuilt with new lists: only a comparison by content keeps the step out.
    [Fact]
    public void SetClasses_SameClasses_RecordsNothing()
    {
        var copies = _draft.Current.Classes.Values
            .Select(copied => copied with { SkillIds = [.. copied.SkillIds], Resources = [.. copied.Resources] })
            .ToList();

        _draft.SetClasses(copies);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetClasses_OnlySkillsChanged_RecordsOneStep()
    {
        _draft.SetClasses([Archer with { SkillIds = ["cleave"] }, Scholar]);

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetClasses_OnlyResourcesChanged_RecordsOneStep()
    {
        var deeper = Scholar.Resources[0] with { Max = 9 };

        _draft.SetClasses([Archer, Scholar with { Resources = [deeper] }]);

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetClasses_Undo_RestoresTheClasses()
    {
        _draft.SetClasses([Archer with { Name = "Bowman" }, Scholar]);

        _draft.Undo();

        Assert.Same(Archer, _draft.Current.Classes["archer"]);
    }
}
