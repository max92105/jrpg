using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Effects;

/// <summary>
/// Every tiered effect as the Effects page edits it (ADR 0045, decision 983): the draft's records in id order, the ones
/// added this session after them; Add starts an empty id, Duplicate copies the chosen one after it under its next tier
/// (decision 985); only an added one can be removed, since skills and items name a saved one (decision 581).
/// <see cref="Problem"/> is an effect's own problem first, then Data's message for the whole list, the skills naming
/// the records included (decision 975). A host chooses an effect by its id.
/// </summary>
internal sealed partial class TieredEffectCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    // Saved effects are never removed or renamed in a session (decision 581), so their ids are read once.
    private readonly IReadOnlyCollection<string> _savedIds;

    /// <param name="content">The draft as it is: the effects edited and the skills naming them.</param>
    public TieredEffectCatalogViewModel(ContentCatalog content)
    {
        _content = content;
        _savedIds = [.. content.TieredEffects.Keys];
        Effects =
        [
            .. content.TieredEffects.Values
                .OrderBy(effect => effect.Id, StringComparer.Ordinal)
                .Select(effect => TieredEffectViewModel.From(effect, content.Skills.Values)),
        ];
        foreach (var effect in Effects)
        {
            effect.Changed += OnEffectChanged;
        }

        SelectedEffect = Effects.FirstOrDefault();
    }

    public ObservableCollection<TieredEffectViewModel> Effects { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DuplicateEffectCommand), nameof(RemoveEffectCommand))]
    public partial TieredEffectViewModel? SelectedEffect { get; set; }

    /// <summary>
    /// Why the game would refuse the effects as typed, naming the effect by id (by position while it has none), or a
    /// skill naming one; null when the game would take them.
    /// </summary>
    public string? Problem =>
        RecordListProblem.FirstOf(Effects, effect => effect.Id, effect => effect.Problem, "Effect")
        ?? CatalogProblems.OfTieredEffects([.. Effects.Select(effect => effect.ToEffect())], _content);

    public string? ChosenKey => SelectedEffect?.Id;

    public void Choose(string key) => SelectedEffect = Effects.FirstOrDefault(effect => effect.Id == key) ?? SelectedEffect;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<TieredEffect> ToEffects() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Effects.Select(effect => effect.ToEffect())];

    /// <summary>Adds an effect last and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddEffect() => Insert(TieredEffectViewModel.New(_savedIds), Effects.Count);

    /// <summary>
    /// Adds a copy of the chosen effect after it, its id the next tier no typed effect takes (decision 985), or empty
    /// while the chosen one's id is: an empty stem would make the copy's id "_1".
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasSelectedEffect))]
    private void DuplicateEffect()
    {
        var chosen = SelectedEffect!;
        var nextId = chosen.Id.Length > 0 ? TierIds.Next(chosen.Id, Effects.Select(effect => effect.Id)) : "";
        Insert(TieredEffectViewModel.DuplicateOf(chosen, nextId, _savedIds), Effects.IndexOf(chosen) + 1);
    }

    [RelayCommand(CanExecute = nameof(CanRemoveEffect))]
    private void RemoveEffect()
    {
        var removed = SelectedEffect!;
        removed.Changed -= OnEffectChanged;
        SelectedEffect = ListOrder.Remove(Effects, removed);
        OnPropertyChanged(nameof(Problem));
    }

    private bool HasSelectedEffect() => SelectedEffect is not null;

    private bool CanRemoveEffect() => SelectedEffect is { IsNew: true };

    private void Insert(TieredEffectViewModel effect, int index)
    {
        effect.Changed += OnEffectChanged;
        Effects.Insert(index, effect);
        SelectedEffect = effect;
        OnPropertyChanged(nameof(Problem));
    }

    // Only the typed fields reach here, so Data's check runs once per change.
    private void OnEffectChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Problem));
}
