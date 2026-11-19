using Jrpg.Core.Maps;
using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// The Encounters page's maps: those rolling the chosen encounter, Link and Remove changing a map at once.
public sealed class EncounterCatalogViewModelMapLinkTests
{
    // Out of name order, so the list's sort shows.
    private readonly List<MapDefinition> _maps =
        [MapRolling("ruins", "a_trio"), MapRolling("cave", "a_trio", "b_pair"), MapRolling("den", "b_pair")];
    private readonly EncounterCatalogViewModel _catalog;

    public EncounterCatalogViewModelMapLinkTests()
    {
        _catalog = new EncounterCatalogViewModel(Catalog, Choices, new EncounterMapLinks(() => _maps, SetSettings));
        _catalog.Choose("a_trio");
    }

    [Fact]
    public void LinkedMaps_ChosenEncounter_ListsTheMapsRollingItByName()
    {
        Assert.Equal(["cave", "ruins"], _catalog.LinkedMaps.Select(map => map.MapId));
    }

    [Fact]
    public void LinkedMaps_AnotherEncounterChosen_ListsItsMaps()
    {
        _catalog.Choose("b_pair");

        Assert.Equal(["cave", "den"], _catalog.LinkedMaps.Select(map => map.MapId));
    }

    [Fact]
    public void LinkedMaps_NewEncounter_ListsNoneAndCantLink()
    {
        _catalog.AddEncounterCommand.Execute(null);

        Assert.Empty(_catalog.LinkedMaps);
        Assert.False(_catalog.CanLinkMaps);
        Assert.Throws<InvalidOperationException>(() => _catalog.StartLinkingMap());
    }

    [Fact]
    public void StartLinkingMap_ChosenEncounter_OffersOnlyTheMapsNotRollingIt()
    {
        Assert.Equal(["den"], _catalog.StartLinkingMap().Maps.Select(map => map.MapId));
    }

    [Fact]
    public void LinkMap_PickedMap_ChangesTheMapAndListsIt()
    {
        var link = _catalog.StartLinkingMap();
        link.SelectedMap = link.Maps.Single();

        _catalog.LinkMap(link);

        Assert.Equal(["b_pair", "a_trio"], _maps.Single(map => map.Id == "den").Settings.RandomEncounters!.EncounterIds);
        Assert.Equal(["cave", "den", "ruins"], _catalog.LinkedMaps.Select(map => map.MapId));
    }

    [Fact]
    public void UnlinkMap_ChosenMap_ChangesTheMapAndUnlistsIt()
    {
        _catalog.SelectedLinkedMap = _catalog.LinkedMaps.Single(map => map.MapId == "cave");

        _catalog.UnlinkMapCommand.Execute(null);

        Assert.Equal(["b_pair"], _maps.Single(map => map.Id == "cave").Settings.RandomEncounters!.EncounterIds);
        Assert.Equal(["ruins"], _catalog.LinkedMaps.Select(map => map.MapId));
        Assert.Null(_catalog.SelectedLinkedMap);
    }

    [Fact]
    public void UnlinkMap_NoMapChosen_IsGreyed()
    {
        Assert.False(_catalog.UnlinkMapCommand.CanExecute(null));
    }

    [Fact]
    public void SelectedLinkedMap_Chosen_LightsRemove()
    {
        var raised = 0;
        _catalog.UnlinkMapCommand.CanExecuteChanged += (_, _) => raised++;

        _catalog.SelectedLinkedMap = _catalog.LinkedMaps[0];

        Assert.Equal(1, raised);
        Assert.True(_catalog.UnlinkMapCommand.CanExecute(null));
    }

    // A map's own step or undo changes its random battles behind the page.
    [Fact]
    public void RefreshLinkedMaps_MapChangedElsewhere_ReadsItAgainAndKeepsTheChosenMap()
    {
        _catalog.SelectedLinkedMap = _catalog.LinkedMaps.Single(map => map.MapId == "ruins");
        SetSettings(MapEncounterLink.Unlinked(_maps.Single(map => map.Id == "cave").Settings, "a_trio"));

        _catalog.RefreshLinkedMaps();

        Assert.Equal(["ruins"], _catalog.LinkedMaps.Select(map => map.MapId));
        Assert.Equal("ruins", _catalog.SelectedLinkedMap?.MapId);
    }

    [Fact]
    public void SelectedEncounter_Changed_RaisesCanLinkMaps()
    {
        List<string?> raised = [];
        _catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.AddEncounterCommand.Execute(null);

        Assert.Contains(nameof(EncounterCatalogViewModel.CanLinkMaps), raised);
    }

    private void SetSettings(MapSettings settings)
    {
        var index = _maps.FindIndex(map => map.Id == settings.Id);
        _maps[index] = new MapDefinition(_maps[index].Layout, settings);
    }
}
