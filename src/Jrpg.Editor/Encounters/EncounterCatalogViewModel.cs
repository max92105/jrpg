using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// Every encounter as the Encounters page edits it (decision 905): the draft's encounters in id order, the ones added
/// this session after them; only an added one can be removed, since a map's random battles may roll a saved one
/// (decision 581's rule). <see cref="Problem"/> is the chosen encounter's own first, then another's, then Data's
/// message for the whole list. Its host chooses an encounter by its id (<see cref="IEditedCatalog"/>).
/// </summary>
internal sealed partial class EncounterCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;
    private readonly EncounterChoices _choices;
    private readonly EncounterMapLinks _maps;

    /// <param name="content">The draft as it is: the encounters edited and the enemies they name.</param>
    /// <param name="choices">What each encounter's boxes offer.</param>
    /// <param name="maps">The session's maps, which the chosen encounter is linked to and unlinked from.</param>
    public EncounterCatalogViewModel(ContentCatalog content, EncounterChoices choices, EncounterMapLinks maps)
    {
        _content = content;
        _choices = choices;
        _maps = maps;
        Encounters =
        [
            .. content.Encounters.Values
                .OrderBy(encounter => encounter.Id, StringComparer.Ordinal)
                .Select(encounter => EncounterViewModel.From(encounter, choices)),
        ];
        foreach (var encounter in Encounters)
        {
            encounter.Changed += OnEncounterChanged;
        }

        SelectedEncounter = Encounters.FirstOrDefault();
    }

    public ObservableCollection<EncounterViewModel> Encounters { get; }

    /// <summary>What the chosen encounter's add box offers: every enemy, in the order the page read them.</summary>
    public IReadOnlyList<EnemyChoice> OfferedEnemies => _choices.Enemies;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    [NotifyCanExecuteChangedFor(nameof(RemoveEncounterCommand))]
    public partial EncounterViewModel? SelectedEncounter { get; set; }

    /// <summary>
    /// Why the game would refuse the encounters as typed: the chosen encounter's own first, as the page shows it, then
    /// another's named by id (by position while it has none), then Data's; null when the game would take them.
    /// </summary>
    public string? Problem =>
        SelectedEncounter?.Problem
        ?? RecordListProblem.FirstOf(Encounters, encounter => encounter.Id, encounter => encounter.Problem, "Encounter")
        ?? CatalogProblems.OfEncounters([.. Encounters.Select(encounter => encounter.ToEncounter())], _content);

    /// <summary>The chosen encounter's id; null with none chosen or while a new one has no id yet.</summary>
    public string? ChosenKey => SelectedEncounter is { Id.Length: > 0 } encounter ? encounter.Id : null;

    public void Choose(string key) =>
        SelectedEncounter = Encounters.FirstOrDefault(encounter => encounter.Id == key) ?? SelectedEncounter;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<EncounterDefinition> ToEncounters() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : [.. Encounters.Select(encounter => encounter.ToEncounter())];

    /// <summary>Adds a new encounter last and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddEncounter()
    {
        var encounter = EncounterViewModel.New(_choices);
        encounter.Changed += OnEncounterChanged;
        Encounters.Add(encounter);
        SelectedEncounter = encounter;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveEncounter))]
    private void RemoveEncounter()
    {
        var removed = SelectedEncounter!;
        removed.Changed -= OnEncounterChanged;
        SelectedEncounter = ListOrder.Remove(Encounters, removed);
    }

    private bool CanRemoveEncounter() => SelectedEncounter is { IsNew: true };

    // Only the typed id, the slots and the boxes reach here, never an encounter's label or problem notices, so Data's
    // check runs once per change.
    private void OnEncounterChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Problem));
}
