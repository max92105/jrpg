using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Statuses;

/// <summary>
/// Every status as the Statuses page edits it (ADR 0045, decision 984): the draft's records in id order, the ones added
/// this session after them; Add starts an empty id, Duplicate copies the chosen one after it with its id to type, New
/// tier copies it as its family's next tier (decision 985); only an added one can be removed, since skills and items
/// grant a saved one (decision 581). <see cref="Problem"/> is a status's own problem first, then Data's message for the
/// whole list, a family whose records differ and the skills granting the records included (decision 975). A host
/// chooses a status by its id.
/// </summary>
internal sealed partial class StatusCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    // Saved statuses are never removed or renamed in a session (decision 581), so their ids are read once.
    private readonly IReadOnlyCollection<string> _savedIds;

    /// <param name="content">The draft as it is: the statuses edited and the skills granting them.</param>
    public StatusCatalogViewModel(ContentCatalog content)
    {
        _content = content;
        _savedIds = [.. content.Statuses.Keys];
        Statuses =
        [
            .. content.Statuses.Values
                .OrderBy(status => status.Id, StringComparer.Ordinal)
                .Select(status => StatusViewModel.From(status, content.Skills.Values)),
        ];
        foreach (var status in Statuses)
        {
            status.Changed += OnStatusChanged;
        }

        SelectedStatus = Statuses.FirstOrDefault();
    }

    public ObservableCollection<StatusViewModel> Statuses { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Families))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateStatusCommand), nameof(NewTierCommand), nameof(RemoveStatusCommand))]
    public partial StatusViewModel? SelectedStatus { get; set; }

    /// <summary>
    /// Every family key the statuses have as typed, sorted, for the Family box to offer; raised when another status is
    /// chosen, never as a family is typed, so the box keeps the text being typed.
    /// </summary>
    public IReadOnlyList<string> Families =>
        [.. Statuses.Select(status => status.FamilyKey).Where(key => key.Length > 0).Distinct().Order(StringComparer.Ordinal)];

    /// <summary>
    /// Why the game would refuse the statuses as typed, naming the status by id (by position while it has none), its
    /// family, or a skill granting one; null when the game would take them.
    /// </summary>
    public string? Problem =>
        RecordListProblem.FirstOf(Statuses, status => status.Id, status => status.Problem, "Status")
        ?? CatalogProblems.OfStatuses([.. Statuses.Select(status => status.ToStatus())], _content);

    public string? ChosenKey => SelectedStatus?.Id;

    public void Choose(string key) => SelectedStatus = Statuses.FirstOrDefault(status => status.Id == key) ?? SelectedStatus;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<StatusDefinition> ToStatuses() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Statuses.Select(status => status.ToStatus())];

    /// <summary>Adds a status last and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddStatus() => Insert(StatusViewModel.New(_savedIds), Statuses.Count);

    /// <summary>Adds a copy of the chosen status after it, its id to type.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedStatus))]
    private void DuplicateStatus()
    {
        var chosen = SelectedStatus!;
        Insert(StatusViewModel.DuplicateOf(chosen, "", _savedIds), Statuses.IndexOf(chosen) + 1);
    }

    /// <summary>
    /// Adds a copy of the chosen status after it in the chosen one's family, so the family's parts and timing still
    /// match, its id the next tier no typed status takes (decision 985), or empty while the chosen one's id is: an
    /// empty stem would make the copy's id "_1".
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasSelectedStatus))]
    private void NewTier()
    {
        var chosen = SelectedStatus!;
        var nextId = chosen.Id.Length > 0 ? TierIds.Next(chosen.Id, Statuses.Select(status => status.Id)) : "";
        var tier = StatusViewModel.DuplicateOf(chosen, nextId, _savedIds);
        tier.FamilyText = chosen.FamilyKey;
        Insert(tier, Statuses.IndexOf(chosen) + 1);
    }

    [RelayCommand(CanExecute = nameof(CanRemoveStatus))]
    private void RemoveStatus()
    {
        var removed = SelectedStatus!;
        removed.Changed -= OnStatusChanged;
        SelectedStatus = ListOrder.Remove(Statuses, removed);
        OnPropertyChanged(nameof(Problem));
    }

    private bool HasSelectedStatus() => SelectedStatus is not null;

    private bool CanRemoveStatus() => SelectedStatus is { IsNew: true };

    private void Insert(StatusViewModel status, int index)
    {
        status.Changed += OnStatusChanged;
        Statuses.Insert(index, status);
        SelectedStatus = status;
        OnPropertyChanged(nameof(Problem));
    }

    // Only the typed fields reach here, so Data's check runs once per change.
    private void OnStatusChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Problem));
}
