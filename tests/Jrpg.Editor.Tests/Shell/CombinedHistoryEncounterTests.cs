using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Shell;

// An undo that would take away an encounter a map's random battles roll is refused naming the map (decision 907, as
// decision 581 refuses an opened shop); a refused undo leaves the encounter and its step in place.
public sealed class CombinedHistoryEncounterTests : IDisposable
{
    private static readonly EncounterDefinition _wisps = new() { Id = "test_wisps", EnemyIds = ["gnoll"] };

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    // As the page then Map > Settings would: the encounter first, then the shown forest set to roll it, unsaved;
    // showing another map leaves the encounter's step the next undo.
    [Fact]
    public void Undo_AddedEncounterRolledOnAnOpenMap_IsRefusedNamingTheMap()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.Content!.SetEncounters([.. viewModel.Catalog!.Encounters.Values, _wisps]);
        var forest = viewModel.CurrentMap!;
        Roll(forest, "test_wisps");
        viewModel.TryOpenMap("start");

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal("Can't undo: encounter test_wisps is rolled on map forest.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Encounters.ContainsKey("test_wisps"));
        Assert.True(viewModel.Content.CanUndo);
    }

    [Fact]
    public void Undo_AddedEncounterNoMapRolls_RemovesIt()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.Content!.SetEncounters([.. viewModel.Catalog!.Encounters.Values, _wisps]);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Null(viewModel.History.Refusal);
        Assert.False(viewModel.Catalog.Encounters.ContainsKey("test_wisps"));
    }

    // The saved to_aslas_1, never opened, rolls goblin_chief: taken out then put back as two steps (the page never
    // removes a saved encounter, decision 905, so the draft is called directly), the next undo would remove it.
    [Fact]
    public void Undo_AddedEncounterRolledOnlyOnAnUnopenedSavedMap_IsRefused()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var encounters = viewModel.Catalog!.Encounters.Values.ToList();
        viewModel.Content!.SetEncounters([.. encounters.Where(encounter => encounter.Id != "goblin_chief")]);
        viewModel.Content.SetEncounters(encounters);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal("Can't undo: encounter goblin_chief is rolled on map to_aslas_1.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Encounters.ContainsKey("goblin_chief"));
    }

    private static void Roll(MapViewModel map, string encounterId)
    {
        var settings = map.Map.Settings;
        var battles = new RandomEncounterTable { OneInSteps = 20, GraceSteps = 3, EncounterIds = [encounterId] };
        map.Do(new SetMapSettingsEdit(settings, settings with { RandomEncounters = battles }));
    }
}
