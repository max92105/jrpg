using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// The Encounters page's list (decision 905): the encounters in id order, the added ones after, only an added one
// removable, and the problem line: the chosen encounter's own, then another's, then Data's message for the list.
public sealed class EncounterCatalogViewModelTests
{
    [Fact]
    public void Constructor_Encounters_InIdOrder()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);

        Assert.Equal(["a_trio", "b_pair"], catalog.Encounters.Select(encounter => encounter.Id));
        Assert.Same(catalog.Encounters[0], catalog.SelectedEncounter);
    }

    [Fact]
    public void Problem_Constructed_IsNull()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void AddEncounter_NewEncounter_LastAndChosen()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);

        catalog.AddEncounterCommand.Execute(null);

        var added = catalog.Encounters[^1];
        Assert.Same(added, catalog.SelectedEncounter);
        Assert.Equal((true, ""), (added.IsNew, added.Id));
    }

    [Fact]
    public void RemoveEncounter_SavedEncounter_CannotExecute()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);

        Assert.False(catalog.RemoveEncounterCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveEncounter_AddedEncounter_RemovesItAndChoosesTheNext()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);
        catalog.AddEncounterCommand.Execute(null);
        var first = catalog.SelectedEncounter!;
        catalog.AddEncounterCommand.Execute(null);
        var second = catalog.SelectedEncounter!;
        catalog.SelectedEncounter = first;

        catalog.RemoveEncounterCommand.Execute(null);

        Assert.DoesNotContain(first, catalog.Encounters);
        Assert.Same(second, catalog.SelectedEncounter);
    }

    [Fact]
    public void CanExecuteChanged_AddedEncounterChosen_LightsRemove()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);
        var raised = 0;
        catalog.RemoveEncounterCommand.CanExecuteChanged += (_, _) => raised++;

        catalog.AddEncounterCommand.Execute(null);

        Assert.Equal(1, raised);
        Assert.True(catalog.RemoveEncounterCommand.CanExecute(null));
    }

    [Fact]
    public void Problem_TwoEncountersSameId_NamesTheLaterOne()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);
        AddWithAnOrc(catalog, "a_trio");

        catalog.SelectedEncounter = catalog.Encounters[1];

        Assert.Equal("Encounter a_trio is listed twice.", catalog.Problem);
    }

    // The new one's problem is named by position on the list, while the chosen one's own shows as the page shows it.
    [Fact]
    public void Problem_AnotherChosen_RaisedAndNamesTheNewOneByPosition()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);
        catalog.AddEncounterCommand.Execute(null);
        Assert.Equal("The id must not be empty.", catalog.Problem);
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        catalog.SelectedEncounter = catalog.Encounters[0];

        Assert.Contains(nameof(EncounterCatalogViewModel.Problem), raised);
        Assert.Equal("Encounter #3: The id must not be empty.", catalog.Problem);
    }

    // A slot added to an encounter that isn't chosen fixes its problem: the line clears, and its change is raised.
    [Fact]
    public void Problem_EnemyAddedToAnotherEncounter_Refreshes()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);
        catalog.AddEncounterCommand.Execute(null);
        var added = catalog.SelectedEncounter!;
        added.Id = "c_lone";
        catalog.SelectedEncounter = catalog.Encounters[0];
        Assert.Equal("Encounter c_lone: An encounter needs at least one enemy.", catalog.Problem);
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        added.EnemyToAdd = Orc;
        added.AddEnemyCommand.Execute(null);

        Assert.Contains(nameof(EncounterCatalogViewModel.Problem), raised);
        Assert.Null(catalog.Problem);
    }

    // The boxes offer an enemy the content lacks: only Data's check of the whole list sees it.
    [Fact]
    public void Problem_EnemyTheContentLacks_IsDatasMessage()
    {
        var dragon = new EnemyChoice("dragon", "Dragon");
        var catalog = new EncounterCatalogViewModel(Catalog, Choices with { Enemies = [Goblin, Orc, dragon] }, NoMaps);
        var trio = catalog.Encounters[0];

        trio.EnemyToAdd = dragon;
        trio.AddEnemyCommand.Execute(null);

        Assert.Equal("encounters.json: encounter 'a_trio': enemyIds has unknown enemy id 'dragon'.", catalog.Problem);
    }

    [Fact]
    public void ToEncounters_AddedEncounter_Included()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);
        AddWithAnOrc(catalog, "c_lone");

        var written = catalog.ToEncounters();

        Assert.Equal(["a_trio", "b_pair", "c_lone"], written.Select(encounter => encounter.Id));
        Assert.Equal(["orc"], written[2].EnemyIds);
    }

    [Fact]
    public void ToEncounters_WithAProblem_Throws()
    {
        var catalog = new EncounterCatalogViewModel(Catalog, Choices, NoMaps);

        catalog.AddEncounterCommand.Execute(null);

        Assert.Throws<InvalidOperationException>(catalog.ToEncounters);
    }

    private static void AddWithAnOrc(EncounterCatalogViewModel catalog, string id)
    {
        catalog.AddEncounterCommand.Execute(null);
        var added = catalog.SelectedEncounter!;
        added.Id = id;
        added.EnemyToAdd = Orc;
        added.AddEnemyCommand.Execute(null);
    }
}
