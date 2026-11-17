using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Party;

/// <summary>
/// One battle resource of a class as the Resources tab types it (decision 950): its id (typed while new, fixed once the
/// draft holds it, since skills name it), name, color, the lasting box, its amounts (a max and a start, or from speed
/// with the speed per charge, which turns the other two off) and its gains, written over the record it came from, so
/// the four rules no box edits yet are carried as they are. Only the boxes are its own; Data's rules (a start within
/// the max, a color's shape, the heavy-hit share's range) are the class list's line.
/// </summary>
internal sealed partial class ClassResourceViewModel : ObservableObject
{
    // What the box shows for a resource with fixed amounts, so ticking From speed starts on a legal number: the
    // validator's lowest, as the blank's (decision 1015).
    private const int LowestSpeedPerCharge = 1;

    // Add's starting resource (design section 1).
    private static readonly ResourceDefinition _blank = new("", "", Max: 1, StartsAt: 0) { Color = "#FFFFFF" };

    private readonly ResourceDefinition _original;

    private ClassResourceViewModel(ResourceDefinition original)
    {
        _original = original;
        Id = original.Id;
        Name = original.Name;
        ColorText = original.Color;
        IsLasting = original.IsLasting;
        TakesMaxFromSpeed = original.MaxFromSpeed is not null;
        MaxText = TypedNumber.Text(original.Max);
        StartsAtText = TypedNumber.Text(original.StartsAt);
        SpeedPerChargeText = TypedNumber.Text(original.MaxFromSpeed?.SpeedPerCharge ?? LowestSpeedPerCharge);
        Gains = new ResourceGainsBoxes(original.Gains);
        Gains.PropertyChanged += OnGainTyped;
    }

    /// <summary>
    /// A typed field or gain: what the class's list listens to, never the label's notice, so the list runs Data's check
    /// once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="resource">The resource as the draft holds it.</param>
    public static ClassResourceViewModel From(ResourceDefinition resource) => new(resource);

    /// <summary>
    /// A resource added this session: its id and name left to type, white, a max of 1, no gains, no rules.
    /// </summary>
    public static ClassResourceViewModel New() => new(_blank) { IsNew = true };

    /// <summary>
    /// A copy of the resource as typed, for a duplicated class (decision 585): the texts, a half-typed number included,
    /// and the id too, since the class's skills name it; new, as the draft holds no resource of the new class.
    /// </summary>
    public static ClassResourceViewModel DuplicateOf(ClassResourceViewModel chosen)
    {
        var copy = new ClassResourceViewModel(chosen._original)
        {
            IsNew = true,
            Id = chosen.Id,
            Name = chosen.Name,
            ColorText = chosen.ColorText,
            IsLasting = chosen.IsLasting,
            TakesMaxFromSpeed = chosen.TakesMaxFromSpeed,
            MaxText = chosen.MaxText,
            StartsAtText = chosen.StartsAtText,
            SpeedPerChargeText = chosen.SpeedPerChargeText,
        };
        copy.Gains.CopyFrom(chosen.Gains);
        return copy;
    }

    /// <summary>True while the resource was added this session: the only time its id can be typed.</summary>
    public bool IsNew { get; private init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Name { get; set; }

    /// <summary>The color its pips or bar are drawn in, as typed; Data checks the <c>#RRGGBB</c> shape.</summary>
    [ObservableProperty]
    public partial string ColorText { get; set; }

    [ObservableProperty]
    public partial bool IsLasting { get; set; }

    /// <summary>
    /// Set when the max and start are the holder's speed charges: the max and start boxes then go unused.
    /// </summary>
    [ObservableProperty]
    public partial bool TakesMaxFromSpeed { get; set; }

    [ObservableProperty]
    public partial string MaxText { get; set; }

    [ObservableProperty]
    public partial string StartsAtText { get; set; }

    /// <summary>The speed one charge takes; used only while <see cref="TakesMaxFromSpeed"/>.</summary>
    [ObservableProperty]
    public partial string SpeedPerChargeText { get; set; }

    public ResourceGainsBoxes Gains { get; }

    /// <summary>The list's line: "mana: Mana"; "(new resource)" while it has no id.</summary>
    public string Label => Id.Length > 0 ? $"{Id}: {Name}" : "(new resource)";

    /// <summary>
    /// Why the resource can't be built: the typed id while new, then the first amount box that counts and isn't a whole
    /// number, then the first gain's; null when it can.
    /// </summary>
    /// <param name="classmateIds">The class's other resources' ids, which a new id must not repeat.</param>
    public string? ProblemAmong(IReadOnlyCollection<string> classmateIds) =>
        (IsNew ? ObjectIds.ProblemOf(Id, classmateIds) : null) ?? BoxesProblem;

    /// <summary>
    /// The record it came from with the typed fields over it, the name and color trimmed; the max and start 0 while it
    /// takes them from speed, as Data asks. The id isn't checked here: the class's list checks it against the others.
    /// </summary>
    /// <exception cref="InvalidOperationException">A box isn't a whole number.</exception>
    public ResourceDefinition ToResource() =>
        BoxesProblem is { } problem
            ? throw new InvalidOperationException(problem)
            : _original with
            {
                Id = Id,
                Name = Name.Trim(),
                Color = ColorText.Trim(),
                IsLasting = IsLasting,
                Max = TakesMaxFromSpeed ? 0 : NumberIn(MaxText),
                StartsAt = TakesMaxFromSpeed ? 0 : NumberIn(StartsAtText),
                MaxFromSpeed = TakesMaxFromSpeed ? new SpeedScaling(NumberIn(SpeedPerChargeText)) : null,
                Gains = Gains.Typed!,
            };

    // Only the amount boxes that count: a half-typed max under From speed is never written.
    private string? BoxesProblem =>
        (TakesMaxFromSpeed
            ? TypedNumber.ProblemOf("Speed per charge", SpeedPerChargeText)
            : TypedNumber.ProblemOf("Max", MaxText) ?? TypedNumber.ProblemOf("Starts at", StartsAtText))
        ?? Gains.Problem;

    private static int NumberIn(string text) => TypedNumber.Parse(text)!.Value;

    // Every typed gain notifies the boxes' Problem, so that notice alone stands for one typed change.
    private void OnGainTyped(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ResourceGainsBoxes.Problem))
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    partial void OnIdChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnNameChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnColorTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIsLastingChanged(bool value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnTakesMaxFromSpeedChanged(bool value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnMaxTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnStartsAtTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnSpeedPerChargeTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
