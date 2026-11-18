using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Progression;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Progression;

/// <summary>
/// The experience table as the Experience page types it (decision 956): one row per level step, each with its amount as
/// typed and its running total, a level added after the last or the last removed. <see cref="Problem"/> is the first
/// box that isn't a whole number, then Data's message on the draft's rules with the typed table;
/// <see cref="MaxLevelNote"/> warns of a max level below the saved one. One table: its host has nothing to choose
/// (<see cref="IEditedCatalog"/>).
/// </summary>
internal sealed partial class ExperienceTableViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    private readonly int _savedMaxLevel;

    /// <param name="content">
    /// The draft as it is: its rules' table edited, and the classes whose skills it must reach.
    /// </param>
    /// <param name="savedMaxLevel">
    /// The max level <c>rules.json</c> holds (<see cref="CatalogDraft.SavedMaxLevel"/>), which may differ from the
    /// draft's once a lower table is applied and not yet saved.
    /// </param>
    public ExperienceTableViewModel(ContentCatalog content, int savedMaxLevel)
    {
        _content = content;
        _savedMaxLevel = savedMaxLevel;
        Rows =
        [
            .. content.Rules.Experience.ExperienceToNextLevel.Select(
                (amount, index) => new ExperienceRowViewModel(index + 1, TypedNumber.Text(amount))),
        ];
        ListItemWatch.WatchRowsOf(
            Rows,
            row => row.PropertyChanged += OnRowTyped,
            row => row.PropertyChanged -= OnRowTyped,
            OnRowsListChanged);
        UpdateTotals();
    }

    /// <summary>The level steps from level 1 on: their count sets the max level.</summary>
    public ObservableCollection<ExperienceRowViewModel> Rows { get; }

    /// <summary>One past the last step, as the game reads the table.</summary>
    public int MaxLevel => Rows.Count + 1;

    /// <summary>
    /// "Max level 18, was 21: saved members above it drop to 18 when a save loads." while the table has fewer levels
    /// than the saved table, even after an apply (a member saved past the max level is brought down at load, decision
    /// 943); null at the same or more levels.
    /// </summary>
    public string? MaxLevelNote =>
        MaxLevel < _savedMaxLevel
            ? $"Max level {MaxLevel}, was {_savedMaxLevel}: saved members above it drop to {MaxLevel} when a save loads."
            : null;

    /// <summary>
    /// Why the game would refuse the table as typed: the first row that isn't a whole number, then Data's message
    /// (<see cref="CatalogProblems.OfRules"/>: at least one entry, each at least 1, no class skill past the max level);
    /// null when the game would take it.
    /// </summary>
    public string? Problem =>
        Rows.Select(row => row.Problem).FirstOrDefault(problem => problem is not null)
        ?? CatalogProblems.OfRules(_content.Rules with { Experience = AsTyped() }, _content);

    /// <summary>Always null: the table is one record, so there is nothing to choose.</summary>
    public string? ChosenKey => null;

    /// <summary>Keeps everything as it is: the table is the page's one record.</summary>
    public void Choose(string key)
    {
    }

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public ExperienceTable ToTable() => Problem is { } problem ? throw new InvalidOperationException(problem) : AsTyped();

    /// <summary>Appends the next level step, its amount copied from the last row as typed.</summary>
    [RelayCommand]
    private void AddLevel() => Rows.Add(new ExperienceRowViewModel(MaxLevel, Rows[^1].AmountText));

    // Greyed at one row: Data refuses an empty table.
    [RelayCommand(CanExecute = nameof(HasMoreThanOneRow))]
    private void RemoveLastLevel() => Rows.RemoveAt(Rows.Count - 1);

    private bool HasMoreThanOneRow() => Rows.Count > 1;

    // Read only once every box is a number.
    private ExperienceTable AsTyped() => new([.. Rows.Select(row => row.Amount!.Value)]);

    private void OnRowsListChanged()
    {
        RemoveLastLevelCommand.NotifyCanExecuteChanged();
        UpdateTotals();
        OnPropertyChanged(nameof(MaxLevel));
        OnPropertyChanged(nameof(MaxLevelNote));
        OnPropertyChanged(nameof(Problem));
    }

    // Only the typed amount, never the row's total, which this table sets, so Data's check runs once per change.
    private void OnRowTyped(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExperienceRowViewModel.AmountText))
        {
            UpdateTotals();
            OnPropertyChanged(nameof(Problem));
        }
    }

    // A box that can't be read blanks its own total and every one below it.
    private void UpdateTotals()
    {
        long? total = 0;
        foreach (var row in Rows)
        {
            total = row.Amount is { } amount ? total + amount : null;
            row.TotalToReach = total;
        }
    }
}
