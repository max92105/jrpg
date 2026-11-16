using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Statuses;

/// <summary>
/// One status as the Statuses page edits it (ADR 0055, decision 984): its id (typed while the status is new, fixed once
/// the draft holds it, decision 582), name, description, harmful flag, family, timing, mark letter and parts as typed,
/// with the skills granting it as a read-only line. <see cref="Problem"/> is only the typed id's and the part rows':
/// the rest is Data's rules, which the list shows for every status at once.
/// </summary>
internal sealed partial class StatusViewModel : ObservableObject
{
    // The boxes start on the record's fields, an absent family or letter as an empty box: a new status's id is empty.
    private StatusViewModel(StatusDefinition start, IEnumerable<StatusPartRow> parts, IReadOnlyList<string> userIds)
    {
        IdText = start.Id;
        Name = start.Name;
        Description = start.Description;
        IsHarmful = start.IsHarmful;
        FamilyText = start.Family ?? "";
        ActsAt = start.ActsAt;
        MarkLetterText = start.MarkLetter ?? "";
        UsedByText = userIds.Count == 0 ? "Used by no skill" : $"Used by: {string.Join(", ", userIds)}";
        Parts = [.. parts];
        foreach (var part in Parts)
        {
            part.Changed += OnPartChanged;
        }

        Parts.CollectionChanged += OnPartsListChanged;
        SelectedPart = Parts.FirstOrDefault();
    }

    /// <summary>
    /// A typed box changed, a part's included, or a part was added, moved or removed: what the list's problem line
    /// listens to, never the id's, the line's or the problem's own notices, so Data's check runs once per edit.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="status">The record as the draft holds it.</param>
    /// <param name="skills">The draft's skills, whose effects' grants name the records.</param>
    public static StatusViewModel From(StatusDefinition status, IEnumerable<SkillDefinition> skills) =>
        new(
            status,
            status.Parts.Select(StatusPartRow.From),
            [
                .. skills
                    .Where(skill => skill.Effect.Statuses.Any(grant => grant.Status.Id == status.Id))
                    .Select(skill => skill.Id)
                    .Order(StringComparer.Ordinal),
            ]);

    /// <summary>
    /// A status as Add starts it: its id and name empty, a harmful one ticking at the end of its holder's turn with one
    /// part as an added row starts, so the id and name are what is left to type.
    /// </summary>
    /// <param name="takenIds">The saved statuses' ids, which the typed id must not take.</param>
    public static StatusViewModel New(IReadOnlyCollection<string> takenIds) =>
        new(
            new StatusDefinition { Id = "", Name = "", Description = "", IsHarmful = true, Parts = [] },
            [StatusPartRow.New()],
            [])
        { IsNew = true, TakenIds = takenIds };

    /// <summary>
    /// A new status copying the chosen one's boxes and part rows as typed, so a half-typed one duplicates too, under
    /// the id given (decision 985: the chosen one's next tier, or empty for a plain copy), still editable.
    /// </summary>
    /// <param name="chosen">The status chosen in the list, saved or added.</param>
    /// <param name="nextId">The id the copy starts with.</param>
    /// <param name="takenIds">The saved statuses' ids, which the typed id must not take.</param>
    public static StatusViewModel DuplicateOf(
        StatusViewModel chosen,
        string nextId,
        IReadOnlyCollection<string> takenIds) =>
        new(
            chosen.TypedRecord([]) with { Id = nextId },
            chosen.Parts.Select(part => new StatusPartRow(part.Kind, part.AmountText, part.DamageType, part.Stat)),
            [])
        { IsNew = true, TakenIds = takenIds };

    /// <summary>
    /// True while the status was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    public IReadOnlyList<StatusTiming> Timings { get; } = Enum.GetValues<StatusTiming>();

    /// <summary>The id as typed; the page lets it be typed only while <see cref="IsNew"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Id), nameof(Problem))]
    public partial string IdText { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>A short line about what it does; it may be empty.</summary>
    [ObservableProperty]
    public partial string Description { get; set; }

    /// <summary>Whether it hurts its holder, which the game's mark shows; false for a boon.</summary>
    [ObservableProperty]
    public partial bool IsHarmful { get; set; }

    /// <summary>The statuses a holder keeps one of (decision 800); empty for its own id.</summary>
    [ObservableProperty]
    public partial string FamilyText { get; set; }

    /// <summary>When its tick acts in its holder's turn (decision 695).</summary>
    [ObservableProperty]
    public partial StatusTiming ActsAt { get; set; }

    /// <summary>The letter its mark shows (decision 810); empty for the name's first.</summary>
    [ObservableProperty]
    public partial string MarkLetterText { get; set; }

    /// <summary>The parts it applies while held, in the record's order.</summary>
    public ObservableCollection<StatusPartRow> Parts { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemovePartCommand), nameof(MovePartUpCommand), nameof(MovePartDownCommand))]
    public partial StatusPartRow? SelectedPart { get; set; }

    public string Id => IdText.Trim();

    /// <summary>
    /// The family a holder keeps one of, as typed: the family box, else the typed id, as
    /// <see cref="StatusDefinition.FamilyKey"/> reads the record; empty while both are.
    /// </summary>
    public string FamilyKey => AbsentWhenEmpty(FamilyText) ?? Id;

    /// <summary>
    /// "Used by: rot, venom", the draft's skills granting the record, by id, or "Used by no skill"; a new status is
    /// granted by none.
    /// </summary>
    public string UsedByText { get; }

    // The saved statuses' ids, which a new status's typed id must not take: fixed and never removed in a session
    // (decision 581), so read once; a saved status's id isn't typed.
    private IReadOnlyCollection<string> TakenIds { get; init; } = [];

    /// <summary>
    /// Why the status can't be built: the typed id while new, then the first part row's, "Parts row 2: ..."; null when
    /// every box can be read.
    /// </summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, TakenIds) : null) ?? SectionRowProblem.FirstOf("Parts", Parts, part => part.Problem);

    /// <summary>The record as typed, its parts from their rows in order.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public StatusDefinition ToStatus() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : TypedRecord([.. Parts.Select(part => part.ToPart())]);

    // An empty family or letter is absent from the record: the status's own id, the name's first letter.
    private static string? AbsentWhenEmpty(string text) => text.Trim() is { Length: > 0 } trimmed ? trimmed : null;

    private StatusDefinition TypedRecord(IReadOnlyList<StatusPart> parts) =>
        new()
        {
            Id = Id,
            Name = Name.Trim(),
            Description = Description,
            IsHarmful = IsHarmful,
            ActsAt = ActsAt,
            Family = AbsentWhenEmpty(FamilyText),
            MarkLetter = AbsentWhenEmpty(MarkLetterText),
            Parts = parts,
        };

    [RelayCommand]
    private void AddPart()
    {
        var part = StatusPartRow.New();
        ListOrder.InsertAfter(Parts, part, SelectedPart);
        SelectedPart = part;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedPart))]
    private void RemovePart() => SelectedPart = ListOrder.Remove(Parts, SelectedPart!);

    [RelayCommand(CanExecute = nameof(HasSelectedPart))]
    private void MovePartUp() => ListOrder.MoveBy(Parts, SelectedPart!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedPart))]
    private void MovePartDown() => ListOrder.MoveBy(Parts, SelectedPart!, 1);

    private bool HasSelectedPart() => SelectedPart is not null;

    private void OnPartsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<StatusPartRow>(e, part => part.Changed += OnPartChanged, part => part.Changed -= OnPartChanged);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnPartChanged(object? sender, EventArgs e) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIdTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnNameChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDescriptionChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIsHarmfulChanged(bool value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnFamilyTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnActsAtChanged(StatusTiming value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnMarkLetterTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
