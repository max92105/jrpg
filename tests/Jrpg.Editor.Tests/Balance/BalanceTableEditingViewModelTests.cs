using Jrpg.Editor.Balance;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Balance;

// The balance table's session reads the draft as it is and changes nothing in it (decision 992).
public sealed class BalanceTableEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(BalanceFixtures.World);

    private readonly BalanceTableEditingViewModel _editing;

    public BalanceTableEditingViewModelTests() => _editing = new BalanceTableEditingViewModel(_draft);

    [Fact]
    public void Apply_AfterTypingALevel_RecordsNoStep()
    {
        var table = _editing.Start();
        table.FirstLevelText = "5";

        _editing.Apply(table);

        Assert.False(_draft.CanUndo);
    }

    // The mage's Power 12 at level 1: fire_1 at 2 per Power reads 24 before the change, 36 at 3.
    [Fact]
    public void Start_TierChangedInTheDraft_ReadsTheChange()
    {
        _draft.SetTieredEffects([BalanceFixtures.Fire1 with { PerPower = 3 }, BalanceFixtures.Heal1]);

        var table = _editing.Start();

        var fireball = Assert.Single(table.ClassSkillRows, row => row.SkillName == BalanceFixtures.Fireball.Name);
        Assert.Equal("36", fireball.AmountTexts[0]);
    }
}
