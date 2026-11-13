using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Battle;

/// <summary>
/// Every enemy as the Enemies window edits it (ADR 0045): the draft's enemies in id order, the ones added this session
/// after them; Add starts from the tunable defaults, Duplicate from the chosen one (decision 585); only an added one
/// can be removed, since encounters name a saved one (decision 581). <see cref="Problem"/> is an enemy's own problem
/// first, then Data's message for the whole list (decision 580).
/// </summary>
internal sealed partial class EnemyCatalogViewModel : ObservableObject
{
    private readonly ContentCatalog _content;

    /// <param name="content">
    /// The draft as it is: the enemies edited, and the skills and drop tables they may name.
    /// </param>
    public EnemyCatalogViewModel(ContentCatalog content)
    {
        _content = content;
        Enemies = [.. content.Enemies.Values.OrderBy(enemy => enemy.Id, StringComparer.Ordinal).Select(EnemyViewModel.From)];
        foreach (var enemy in Enemies)
        {
            enemy.PropertyChanged += OnEnemyChanged;
        }

        SelectedEnemy = Enemies.FirstOrDefault();
    }

    public ObservableCollection<EnemyViewModel> Enemies { get; }

    /// <summary>The damage types the window's box offers, from Core's list so a type added appears.</summary>
    public IReadOnlyList<DamageType> DamageTypes { get; } = Enum.GetValues<DamageType>();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DuplicateEnemyCommand), nameof(RemoveEnemyCommand))]
    public partial EnemyViewModel? SelectedEnemy { get; set; }

    /// <summary>
    /// Why the game would refuse the enemies as typed, naming the enemy by id (by position while it has none); null
    /// when the game would take them.
    /// </summary>
    public string? Problem =>
        RecordListProblem.FirstOf(Enemies, enemy => enemy.Id, enemy => enemy.Problem, "Enemy")
        ?? CatalogProblems.OfEnemies([.. Enemies.Select(enemy => enemy.ToEnemy())], _content);

    /// <summary>Whether OK may apply the session: no problem shows.</summary>
    public bool CanApply => Problem is null;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the window applies only without one.
    /// </exception>
    public IReadOnlyList<EnemyDefinition> ToEnemies() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Enemies.Select(enemy => enemy.ToEnemy())];

    /// <summary>
    /// Adds an enemy with nothing typed yet, last, and chooses it, so its id is the next thing typed.
    /// </summary>
    [RelayCommand]
    private void AddEnemy() => Insert(EnemyViewModel.New(), Enemies.Count);

    /// <summary>Adds a copy of the chosen enemy after it, its id left to type (decision 585).</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedEnemy))]
    private void DuplicateEnemy() => Insert(EnemyViewModel.DuplicateOf(SelectedEnemy!), Enemies.IndexOf(SelectedEnemy!) + 1);

    [RelayCommand(CanExecute = nameof(CanRemoveEnemy))]
    private void RemoveEnemy()
    {
        var removed = SelectedEnemy!;
        removed.PropertyChanged -= OnEnemyChanged;
        SelectedEnemy = ListOrder.Remove(Enemies, removed);
        RaiseProblemChanged();
    }

    private bool HasSelectedEnemy() => SelectedEnemy is not null;

    private bool CanRemoveEnemy() => SelectedEnemy is { IsNew: true };

    private void Insert(EnemyViewModel enemy, int index)
    {
        enemy.PropertyChanged += OnEnemyChanged;
        Enemies.Insert(index, enemy);
        SelectedEnemy = enemy;
        RaiseProblemChanged();
    }

    // Every field of an enemy is typed, so each change may move the problem line; the lines it raises for its own label
    // and problem come with the field's change and are skipped.
    private void OnEnemyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(EnemyViewModel.Label) or nameof(EnemyViewModel.Problem)))
        {
            RaiseProblemChanged();
        }
    }

    // The problem line and OK's state read the same thing.
    private void RaiseProblemChanged()
    {
        OnPropertyChanged(nameof(Problem));
        OnPropertyChanged(nameof(CanApply));
    }
}
