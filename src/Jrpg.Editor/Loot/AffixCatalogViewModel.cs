using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Loot;

/// <summary>
/// Every affix of the pool as the Affixes page edits it (decision 884): the draft's affixes in id order, the ones added
/// this session after them; only an added one can be removed, since a unique names a saved one and the Items page reads
/// each it names from the pool. <see cref="Problem"/> is the chosen affix's own first, then any affix's, then Data's
/// message for the pool and the uniques naming it: a unique's value may leave its affix's range but not its side of
/// zero (decision 866).
/// </summary>
internal sealed partial class AffixCatalogViewModel : ObservableObject
{
    private readonly ContentCatalog _content;

    /// <param name="content">The draft as it is: the affixes edited, and the items whose uniques name them.</param>
    public AffixCatalogViewModel(ContentCatalog content)
    {
        _content = content;
        Affixes =
        [
            .. content.Affixes.Values.OrderBy(affix => affix.Id, StringComparer.Ordinal).Select(AffixViewModel.From),
        ];
        foreach (var affix in Affixes)
        {
            affix.Changed += OnAffixChanged;
        }

        SelectedAffix = Affixes.FirstOrDefault();
    }

    public ObservableCollection<AffixViewModel> Affixes { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateCommand), nameof(RemoveAffixCommand))]
    public partial AffixViewModel? SelectedAffix { get; set; }

    /// <summary>
    /// Why the game would refuse the pool as typed: the chosen affix's boxes first, as the page shows them, then
    /// another affix's named by id (by position while it has none), then Data's; null when the game would take it.
    /// </summary>
    public string? Problem =>
        SelectedAffix?.Problem
        ?? RecordListProblem.FirstOf(Affixes, affix => affix.Id, affix => affix.Problem, "Affix")
        ?? CatalogProblems.OfAffixes([.. Affixes.Select(affix => affix.ToAffix())], _content);

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<AffixDefinition> ToAffixes() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Affixes.Select(affix => affix.ToAffix())];

    /// <summary>Adds a new affix last and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddAffix() => Insert(AffixViewModel.New());

    /// <summary>Adds a copy of the chosen affix last, as typed, its id left to type (decision 585).</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedAffix))]
    private void Duplicate() => Insert(SelectedAffix!.Duplicate());

    [RelayCommand(CanExecute = nameof(CanRemoveAffix))]
    private void RemoveAffix()
    {
        var removed = SelectedAffix!;
        removed.Changed -= OnAffixChanged;
        SelectedAffix = ListOrder.Remove(Affixes, removed);
    }

    private bool HasSelectedAffix() => SelectedAffix is not null;

    private bool CanRemoveAffix() => SelectedAffix is { IsNew: true };

    private void Insert(AffixViewModel affix)
    {
        affix.Changed += OnAffixChanged;
        Affixes.Add(affix);
        SelectedAffix = affix;
    }

    // Only the typed boxes and the kind boxes reach here, never an affix's label or problem notices, so Data's check
    // runs once per change.
    private void OnAffixChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Problem));
}
