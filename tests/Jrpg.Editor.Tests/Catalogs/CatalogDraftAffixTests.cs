using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tests.Loot;

namespace Jrpg.Editor.Tests.Catalogs;

// The affix pool is replaced whole as one undo step that every unique's fixed affixes follow (decision 884), compared
// by content so a page closed unchanged records nothing, the kinds in order included.
public sealed class CatalogDraftAffixTests
{
    private readonly CatalogDraft _draft = new(AffixFixtures.Catalog);

    [Fact]
    public void SetAffixes_StatChanged_UniquesNamingItTakeTheNewStat()
    {
        _draft.SetAffixes(WithFierce(AffixFixtures.Fierce with { Stat = StatKind.LifeSteal }));

        Assert.Equal([new RolledAffix("fierce", StatKind.LifeSteal, 5)], _draft.Current.Items["fierce_blade"].FixedAffixes);
    }

    [Fact]
    public void SetAffixes_Undo_RestoresTheUniquesStats()
    {
        _draft.SetAffixes(WithFierce(AffixFixtures.Fierce with { Stat = StatKind.LifeSteal }));

        _draft.Undo();

        Assert.Equal(AffixFixtures.Fierce, _draft.Current.Affixes["fierce"]);
        Assert.Equal(AffixFixtures.FierceBlade.FixedAffixes, _draft.Current.Items["fierce_blade"].FixedAffixes);
    }

    [Fact]
    public void SetAffixes_KindsReordered_RecordsAStep()
    {
        _draft.SetAffixes(WithFierce(AffixFixtures.Fierce with { Kinds = [.. AffixFixtures.Fierce.Kinds.Reverse()] }));

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetAffixes_SameKindsInANewList_RecordsNoStep()
    {
        _draft.SetAffixes(WithFierce(AffixFixtures.Fierce with { Kinds = [.. AffixFixtures.Fierce.Kinds] }));

        Assert.False(_draft.CanUndo);
    }

    // A unique's value is free of its affix's roll range (decision 866): narrowing the range below it keeps it.
    [Fact]
    public void SetAffixes_RangeNarrowedUnderAUniquesValue_KeepsTheValue()
    {
        _draft.SetAffixes(WithFierce(AffixFixtures.Fierce with { Max = 4 }));

        Assert.Equal(4, _draft.Current.Affixes["fierce"].Max);
        Assert.Equal(AffixFixtures.FierceBlade.FixedAffixes, _draft.Current.Items["fierce_blade"].FixedAffixes);
    }

    [Fact]
    public void SetAffixes_StatChanged_LeavesOtherItemsAsTheyWere()
    {
        var before = _draft.Current.Items.Values.Where(item => !item.IsUnique).ToList();

        _draft.SetAffixes(WithFierce(AffixFixtures.Fierce with { Stat = StatKind.LifeSteal }));

        Assert.All(before, item => Assert.Same(item, _draft.Current.Items[item.Id]));
    }

    // Every affix as the draft holds it, fierce replaced.
    private List<AffixDefinition> WithFierce(AffixDefinition fierce) =>
        [.. _draft.Current.Affixes.Values.Select(affix => affix.Id == fierce.Id ? fierce : affix)];
}
