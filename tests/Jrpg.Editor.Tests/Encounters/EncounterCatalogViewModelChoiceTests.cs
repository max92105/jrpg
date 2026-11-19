using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// The key the page's hosts read back and choose by (IEditedCatalog): the chosen encounter's id. b_pair is never the
// first encounter in id order, so only a choice makes it the chosen one.
public sealed class EncounterCatalogViewModelChoiceTests
{
    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);

        catalog.Choose("b_pair");

        Assert.Equal("b_pair", catalog.SelectedEncounter?.Id);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);
        catalog.Choose("b_pair");

        catalog.Choose("missing");

        Assert.Equal("b_pair", catalog.SelectedEncounter?.Id);
    }

    [Fact]
    public void ChosenKey_EncounterChosen_IsItsId()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);

        catalog.SelectedEncounter = catalog.Encounters.Single(encounter => encounter.Id == "b_pair");

        Assert.Equal("b_pair", catalog.ChosenKey);
    }

    // The host shows a record by its key, and a new encounter has none until its id is typed.
    [Fact]
    public void ChosenKey_NewEncounterWithoutId_IsNull()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);

        catalog.AddEncounterCommand.Execute(null);

        Assert.Null(catalog.ChosenKey);
    }
}
