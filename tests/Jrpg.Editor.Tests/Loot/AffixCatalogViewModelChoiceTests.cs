using Jrpg.Core.Loot;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// The key the page's hosts read back and choose by (IEditedCatalog): the chosen affix's id. Fireproof is never the
// first affix in id order, so only a choice makes it the chosen one.
public sealed class AffixCatalogViewModelChoiceTests
{
    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);

        catalog.Choose("fireproof");

        Assert.Equal("fireproof", catalog.SelectedAffix?.Id);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        catalog.Choose("fireproof");

        catalog.Choose("missing");

        Assert.Equal("fireproof", catalog.SelectedAffix?.Id);
    }

    [Fact]
    public void ChosenKey_AffixChosen_IsItsId()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);

        catalog.SelectedAffix = catalog.Affixes.Single(affix => affix.Id == "fireproof");

        Assert.Equal("fireproof", catalog.ChosenKey);
    }

    [Fact]
    public void ChosenKey_NothingChosen_IsNull()
    {
        var noAffixes = AffixFixtures.Catalog with { Affixes = new Dictionary<string, AffixDefinition>() };

        var catalog = new AffixCatalogViewModel(noAffixes);

        Assert.Null(catalog.ChosenKey);
    }

    // The host shows a record by its key, and a new affix has none until its id is typed.
    [Fact]
    public void ChosenKey_NewAffixWithoutId_IsNull()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);

        catalog.AddAffixCommand.Execute(null);

        Assert.Null(catalog.ChosenKey);
    }
}
