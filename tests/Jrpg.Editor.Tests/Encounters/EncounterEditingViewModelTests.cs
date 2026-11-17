using Jrpg.Core.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// The Encounters page starts from the draft, its boxes read as it starts, and its whole session is one undo step, none
// when nothing changed by content (decision 905).
public sealed class EncounterEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(Catalog);
    private readonly EncounterEditingViewModel _editing;

    public EncounterEditingViewModelTests() => _editing = new EncounterEditingViewModel(_draft, () => Choices);

    [Fact]
    public void Apply_BackgroundChanged_RecordsOneStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _editing.Apply(WithCaveForBPair());

        Assert.Equal(1, steps);
        Assert.Equal("Cave.png", _draft.Current.Encounters["b_pair"].BackgroundFile);
    }

    // Every record is rebuilt with a new enemy list: only a comparison by content keeps the step out.
    [Fact]
    public void Apply_NothingChanged_RecordsNoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _editing.Apply(_editing.Start());

        Assert.Equal(0, steps);
    }

    // The same enemies in another order stand elsewhere on screen: the lists compare by order.
    [Fact]
    public void Apply_SlotsReordered_RecordsOneStep()
    {
        var mixed = new EncounterDefinition { Id = "mixed", EnemyIds = ["goblin", "orc"] };
        var encounters = new Dictionary<string, EncounterDefinition> { [mixed.Id] = mixed };
        var draft = new CatalogDraft(Catalog with { Encounters = encounters });
        var editing = new EncounterEditingViewModel(draft, () => Choices);
        var steps = 0;
        draft.StepRecorded += (_, _) => steps++;
        var edited = editing.Start();
        edited.Encounters[0].MoveEnemyDownCommand.Execute(null);

        editing.Apply(edited);

        Assert.Equal(1, steps);
        Assert.Equal(["orc", "goblin"], draft.Current.Encounters["mixed"].EnemyIds);
    }

    [Fact]
    public void Apply_Undo_RestoresTheEncounter()
    {
        _editing.Apply(WithCaveForBPair());

        _draft.Undo();

        Assert.Null(_draft.Current.Encounters["b_pair"].BackgroundFile);
    }

    // The editing view model is built as the editor starts; the folder is read each time the page opens.
    [Fact]
    public void Start_ImageAddedSinceTheEditorStarted_IsOffered()
    {
        var backgrounds = new List<string> { "Cave.png" };
        var editing = new EncounterEditingViewModel(_draft, () => Choices with { Backgrounds = [.. backgrounds] });
        backgrounds.Add("Swamp.png");

        var edited = editing.Start();

        Assert.Contains("Swamp.png", edited.Encounters[0].BackgroundChoices);
    }

    private EncounterCatalogViewModel WithCaveForBPair()
    {
        var edited = _editing.Start();
        edited.Encounters.Single(encounter => encounter.Id == "b_pair").SelectedBackground = "Cave.png";
        return edited;
    }
}
