using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// The Affixes page starts from the draft and its whole session is one undo step (decision 884).
public sealed class AffixEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(AffixFixtures.Catalog);
    private readonly AffixEditingViewModel _editing;

    public AffixEditingViewModelTests() => _editing = new AffixEditingViewModel(_draft);

    [Fact]
    public void Apply_WeightChanged_IsOneUndoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;
        var edited = _editing.Start();
        edited.Affixes.Single(affix => affix.Id == "fierce").WeightText = "9";

        _editing.Apply(edited);

        Assert.Equal(1, steps);
        Assert.Equal(9, _draft.Current.Affixes["fierce"].Weight);
    }

    // The test world lists its kinds out of enum order, which an untouched affix keeps (decision 1002); every record is
    // rebuilt from its boxes, so only a comparison by content keeps the step out.
    [Fact]
    public void Apply_NothingChanged_RecordsNoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _editing.Apply(_editing.Start());

        Assert.Equal(0, steps);
    }
}
