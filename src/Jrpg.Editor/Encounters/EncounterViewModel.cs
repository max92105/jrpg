using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// One encounter as the Encounters page edits it (decision 905): its id (typed while the encounter is new, fixed once
/// the draft holds it, decision 582) and its enemies in slot order, each added after the chosen slot from the add box,
/// the same enemy any number of times, its Background and Music boxes (decision 906). <see cref="Problem"/> is the
/// typed id's, then an encounter with no enemy; the unknown ids are Data's rules, which the list shows.
/// </summary>
internal sealed partial class EncounterViewModel : ObservableObject
{
    /// <summary>The Background box's first entry: the encounter fights on the map's (decision 892).</summary>
    public const string MapsBackground = "(the map's)";

    /// <summary>The Music box's first entry: the encounter plays the battle theme (decision 420).</summary>
    public const string BattleTheme = "(battle theme)";

    private EncounterViewModel(EncounterDefinition source, EncounterChoices choices)
    {
        Id = source.Id;
        Enemies = [.. source.EnemyIds.Select(enemyId => SlotOf(enemyId, choices))];
        Enemies.CollectionChanged += OnEnemiesChanged;
        SelectedSlot = Enemies.FirstOrDefault();
        BackgroundChoices = FileBoxChoices.Of(source.BackgroundFile, MapsBackground, choices.Backgrounds);
        SelectedBackground = source.BackgroundFile ?? MapsBackground;
        MusicChoices = FileBoxChoices.Of(source.MusicFile, BattleTheme, choices.Tracks);
        SelectedMusic = source.MusicFile ?? BattleTheme;
    }

    /// <summary>
    /// The id typed, a slot added, removed or moved, or a box picked: what the list's problem line listens to, never
    /// the label's or the problem's own notices nor a slot's choice, so the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="encounter">The record as the draft holds it.</param>
    /// <param name="choices">The page's boxes; every enemy the encounter names is among them.</param>
    public static EncounterViewModel From(EncounterDefinition encounter, EncounterChoices choices) => new(encounter, choices);

    /// <summary>An encounter added this session: its id left to type, no enemy yet.</summary>
    public static EncounterViewModel New(EncounterChoices choices) =>
        new(new EncounterDefinition { Id = "", EnemyIds = [] }, choices) { IsNew = true };

    /// <summary>
    /// True while the encounter was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    /// <summary>The enemies in slot order: a slot's place on screen is its position here.</summary>
    public ObservableCollection<EnemySlotRow> Enemies { get; }

    /// <summary>The id as typed; the page lets it be typed only while <see cref="IsNew"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveEnemyCommand), nameof(MoveEnemyUpCommand), nameof(MoveEnemyDownCommand))]
    public partial EnemySlotRow? SelectedSlot { get; set; }

    /// <summary>The add box's choice, added by Add enemy; null while nothing is picked.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddEnemyCommand))]
    public partial EnemyChoice? EnemyToAdd { get; set; }

    /// <summary>
    /// The Background box's entries: <see cref="MapsBackground"/>, the encounter's own image when the folder no longer
    /// holds it, then the backgrounds folder's images.
    /// </summary>
    public IReadOnlyList<string> BackgroundChoices { get; }

    [ObservableProperty]
    public partial string SelectedBackground { get; set; }

    /// <summary>
    /// The Music box's entries: <see cref="BattleTheme"/>, the encounter's own track when the folder no longer holds
    /// it, then the music folder's tracks.
    /// </summary>
    public IReadOnlyList<string> MusicChoices { get; }

    [ObservableProperty]
    public partial string SelectedMusic { get; set; }

    /// <summary>
    /// The list's line: "goblin_trio: Goblin, Goblin, Goblin", the enemies in slot order; "(new encounter)" while it
    /// has no id.
    /// </summary>
    public string Label => Id.Length > 0 ? $"{Id}: {string.Join(", ", Enemies.Select(slot => slot.Name))}" : "(new encounter)";

    /// <summary>Why the encounter can't be built: the typed id while new, then no enemy; null when it can.</summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, []) : null)
        ?? (Enemies.Count == 0 ? "An encounter needs at least one enemy." : null);

    /// <summary>
    /// The encounter as edited, its enemies in slot order; <see cref="MapsBackground"/> and <see cref="BattleTheme"/>
    /// are written as null.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public EncounterDefinition ToEncounter() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new EncounterDefinition
            {
                Id = Id,
                EnemyIds = [.. Enemies.Select(slot => slot.EnemyId)],
                BackgroundFile = SelectedBackground == MapsBackground ? null : SelectedBackground,
                MusicFile = SelectedMusic == BattleTheme ? null : SelectedMusic,
            };

    private static EnemySlotRow SlotOf(string enemyId, EncounterChoices choices) =>
        new(enemyId, choices.Enemies.Single(enemy => enemy.Id == enemyId).Name);

    [RelayCommand(CanExecute = nameof(HasEnemyToAdd))]
    private void AddEnemy()
    {
        var slot = new EnemySlotRow(EnemyToAdd!.Id, EnemyToAdd.Name);
        ListOrder.InsertAfter(Enemies, slot, SelectedSlot);
        SelectedSlot = slot;
    }

    private bool HasEnemyToAdd() => EnemyToAdd is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedSlot))]
    private void RemoveEnemy() => SelectedSlot = ListOrder.Remove(Enemies, SelectedSlot!);

    [RelayCommand(CanExecute = nameof(HasSelectedSlot))]
    private void MoveEnemyUp() => ListOrder.MoveBy(Enemies, SelectedSlot!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedSlot))]
    private void MoveEnemyDown() => ListOrder.MoveBy(Enemies, SelectedSlot!, 1);

    private bool HasSelectedSlot() => SelectedSlot is not null;

    private void OnEnemiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Label));
        OnPropertyChanged(nameof(Problem));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    partial void OnIdChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnSelectedBackgroundChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnSelectedMusicChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
