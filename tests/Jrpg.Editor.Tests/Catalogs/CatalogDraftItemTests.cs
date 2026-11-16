using Jrpg.Core.Items;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tests.Items;

namespace Jrpg.Editor.Tests.Catalogs;

// The items are replaced whole as one undo step, compared by content so a page reopened and closed unchanged records
// nothing, a unique's fixed affixes in order included.
public sealed class CatalogDraftItemTests
{
    private readonly CatalogDraft _draft = new(ItemFixtures.Catalog);

    [Fact]
    public void SetItems_SameRecords_RecordsNoStep()
    {
        _draft.SetItems(WithWarlordAffixes([.. ItemFixtures.WarlordAxe.FixedAffixes]));

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetItems_FixedAffixesReordered_RecordsAStep()
    {
        _draft.SetItems(WithWarlordAffixes([.. ItemFixtures.WarlordAxe.FixedAffixes.Reverse()]));

        Assert.True(_draft.CanUndo);
    }

    // Every item as the draft holds it, the unique rebuilt with the given affixes in a list of its own.
    private List<ItemDefinition> WithWarlordAffixes(List<RolledAffix> affixes) =>
        [.. _draft.Current.Items.Values.Select(item => item.Id == "warlord_axe" ? item with { FixedAffixes = affixes } : item)];
}
