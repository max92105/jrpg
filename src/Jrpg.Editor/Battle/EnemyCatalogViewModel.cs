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
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Battle;

/// <summary>
/// Every enemy as the Enemies page edits it (ADR 0045): the draft's enemies in id order, the ones added this session
/// after them; Add starts from the tunable defaults, Duplicate from the chosen one (decision 585); only an added one
/// can be removed, since encounters name a saved one (decision 581), while a saved one's id can be retyped, the
/// encounters following it (<see cref="RenamedIds"/>). <see cref="Problem"/> is an enemy's own problem first, then
/// Data's message for the whole list (decision 580). An enemy is chosen by its id. A rule's boxes offer every trigger
/// and target, and the skills an enemy can use (decisions 850, 851).
/// </summary>
internal sealed partial class EnemyCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    /// <param name="content">
    /// The draft as it is: the enemies edited, the skills they may name (those an enemy can use offered in a rule's Use
    /// box), and the drop tables they may name.
    /// </param>
    /// <param name="artImagesIn">The PNG names of an art folder by name, for the three art boxes.</param>
    public EnemyCatalogViewModel(ContentCatalog content, Func<string, IReadOnlyList<string>> artImagesIn)
    {
        _content = content;
        ArtChoices = new BattleArtChoices(artImagesIn);
        DropTableChoices = ["", .. content.DropTables.Keys.Order(StringComparer.Ordinal)];
        SkillChoices = TacticSkillChoice.AllIn(content);
        Enemies = [.. content.Enemies.Values.OrderBy(enemy => enemy.Id, StringComparer.Ordinal).Select(EnemyViewModel.From)];
        foreach (var enemy in Enemies)
        {
            enemy.PropertyChanged += OnEnemyChanged;
        }

        SelectedEnemy = Enemies.FirstOrDefault();
    }

    public ObservableCollection<EnemyViewModel> Enemies { get; }

    /// <summary>The damage types the page's box offers, from Core's list so a type added appears.</summary>
    public IReadOnlyList<DamageType> DamageTypes { get; } = Enum.GetValues<DamageType>();

    /// <summary>The drop tables an enemy may roll, the empty one for none first, then the draft's by id.</summary>
    public IReadOnlyList<string> DropTableChoices { get; }

    /// <summary>
    /// What a rule's Use box offers, as <see cref="TacticSkillChoice.AllIn"/> lists the draft's skills.
    /// </summary>
    public IReadOnlyList<TacticSkillChoice> SkillChoices { get; }

    /// <summary>What a rule's When box offers: every trigger, whatever the rule's skill (decision 850).</summary>
    public IReadOnlyList<TacticConditionChoice> ConditionChoices { get; } = TacticConditionChoice.All;

    /// <summary>What a rule's On box offers: every target rule, whatever the rule's skill (decision 850).</summary>
    public IReadOnlyList<TacticTargetChoice> TargetChoices { get; } = TacticTargetChoice.All;

    /// <summary>
    /// The three art folders' files, refreshed after an import; which of an enemy's pictures are missing.
    /// </summary>
    public BattleArtChoices ArtChoices { get; }

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

    public string? ChosenKey => SelectedEnemy?.Id;

    public void Choose(string key) => SelectedEnemy = Enemies.FirstOrDefault(enemy => enemy.Id == key) ?? SelectedEnemy;

    /// <summary>
    /// After an Import... beside an art box: the folders are read again, and the last file copied is picked in that box
    /// for the chosen enemy, and a strip imported from a GIF fills its idle loop's boxes (ADR 0047); nothing picked
    /// when nothing was copied.
    /// </summary>
    /// <param name="artFolder">The battlers, portraits or effects folder the import went to.</param>
    /// <param name="result">What the import copied and refused.</param>
    public void TakeImport(string artFolder, SheetImportResult result)
    {
        if (result.LastImported is not { } file)
        {
            return;
        }

        ArtChoices.Refresh();
        SelectedEnemy!.Art.Pick(artFolder, file);
        if (result.Animations.TryGetValue(file, out var loop))
        {
            SelectedEnemy.Art.TakeLoop(loop);
        }
    }

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<EnemyDefinition> ToEnemies() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Enemies.Select(enemy => enemy.ToEnemy())];

    /// <summary>
    /// Each saved enemy's id that was retyped, with its new one, so the encounters naming it follow; an enemy added
    /// this session has no id to follow.
    /// </summary>
    public IReadOnlyDictionary<string, string> RenamedIds() =>
        Enemies
            .Where(enemy => enemy.SavedId is { } savedId && savedId != enemy.Id)
            .ToDictionary(enemy => enemy.SavedId!, enemy => enemy.Id, StringComparer.Ordinal);

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

    // Every field of an enemy is typed, its rules included (one Tactics notice per rule typed or list changed), so each
    // change may move the problem line; the lines it raises for its own label and problem come with the field's change
    // and are skipped.
    private void OnEnemyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(EnemyViewModel.Label) or nameof(EnemyViewModel.Problem)))
        {
            RaiseProblemChanged();
        }
    }

    private void RaiseProblemChanged() => OnPropertyChanged(nameof(Problem));
}
