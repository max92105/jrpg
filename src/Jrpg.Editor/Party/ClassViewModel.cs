using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Party;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Party;

/// <summary>
/// One class as the Classes page types it (decisions 945-947): its id (typed while the class is new, fixed once the
/// draft holds it, decision 582), name, description, and its base and growth as the same eight boxes each, written over
/// the record it came from, so the skills and resources no tab edits yet are carried as they are. <see cref="Problem"/>
/// is only the typed id's and the boxes'; the rest is Data's rules, which the list shows.
/// </summary>
internal sealed partial class ClassViewModel : ObservableObject
{
    // Add's starting record (design section 1): the validator's lowest legal numbers, as the Enemies page's blank.
    private static readonly ClassDefinition _blank = new("", "", "", new ClassStats(MaxHp: 1, Speed: 0), new ClassStats(0, 0));

    private readonly ClassDefinition _source;

    private ClassViewModel(ClassDefinition source)
    {
        _source = source;
        Id = source.Id;
        Name = source.Name;
        Description = source.Description;
        Base = new ClassStatsBoxes(source.Base, "Base");
        Growth = new ClassStatsBoxes(source.Growth, "Growth");
        Base.PropertyChanged += OnBoxTyped;
        Growth.PropertyChanged += OnBoxTyped;
    }

    /// <summary>
    /// The id, the name, the description or a box typed: what the list's problem line listens to, never the label's or
    /// the problem's own notices, so the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="classDefinition">The record as the draft holds it.</param>
    public static ClassViewModel From(ClassDefinition classDefinition) => new(classDefinition);

    /// <summary>
    /// A class added this session: its id, name and description left to type, the lowest legal numbers.
    /// </summary>
    public static ClassViewModel New() => new(_blank) { IsNew = true };

    /// <summary>
    /// A new class copying the chosen one as typed, its id left to type (decision 585): the boxes' texts, not the
    /// record, so a half-typed class duplicates too; its skills and resources come from the record it came from.
    /// </summary>
    public static ClassViewModel DuplicateOf(ClassViewModel chosen)
    {
        var copy = new ClassViewModel(chosen._source with { Id = "" })
        {
            IsNew = true,
            Name = chosen.Name,
            Description = chosen.Description,
        };
        copy.Base.CopyFrom(chosen.Base);
        copy.Growth.CopyFrom(chosen.Growth);
        return copy;
    }

    /// <summary>
    /// True while the class was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    /// <summary>The id as typed; the page lets it be typed only while <see cref="IsNew"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    /// <summary>The stats at level 1.</summary>
    public ClassStatsBoxes Base { get; }

    /// <summary>What each level after the first adds.</summary>
    public ClassStatsBoxes Growth { get; }

    /// <summary>The list's line: "mage: Mage"; "(new class)" while it has no id.</summary>
    public string Label => Id.Length > 0 ? $"{Id}: {Name}" : "(new class)";

    /// <summary>
    /// Why the class can't be built: the typed id while new, then the first box that isn't a whole number, the base's
    /// before the growth's; null when it can.
    /// </summary>
    public string? Problem => (IsNew ? ObjectIds.ProblemOf(Id, []) : null) ?? Base.Problem ?? Growth.Problem;

    /// <summary>The record it came from with the typed fields over it, the name and description trimmed.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public ClassDefinition ToClass() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : _source with
            {
                Id = Id,
                Name = Name.Trim(),
                Description = Description.Trim(),
                Base = Base.Typed!,
                Growth = Growth.Typed!,
            };

    // Every typed box notifies its boxes' Problem, so that notice alone stands for one typed change.
    private void OnBoxTyped(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClassStatsBoxes.Problem))
        {
            OnPropertyChanged(nameof(Problem));
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    partial void OnIdChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnNameChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDescriptionChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
