using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Events;

/// <summary>
/// One step in the event dialog: the fields of its kind and its one-line label (legacy <c>Describe</c>). Numbers are
/// kept as typed, so a box left empty is reported by <see cref="Problem"/> instead of keeping a stray value; ids are
/// picked from lists, and the game's checks run on save.
/// </summary>
internal sealed partial class EventStepViewModel : ObservableObject
{
    // The legacy label's limit on a dialogue's first line.
    private const int LabelLineLength = 30;

    private EventStepViewModel(EventStepKind kind)
    {
        Kind = kind;
    }

    public EventStepKind Kind { get; }

    public bool IsDialogue => Kind == EventStepKind.Dialogue;

    public bool IsGiveItem => Kind == EventStepKind.GiveItem;

    public bool IsSwapObject => Kind == EventStepKind.SwapObject;

    public bool IsWarp => Kind == EventStepKind.Warp;

    /// <summary>One message box per line; blank lines are left out.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string LinesText { get; set; } = "";

    /// <summary>Empty for no speaker name.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Speaker { get; set; } = "";

    /// <summary>The portrait's file in the portraits folder; empty for none.</summary>
    [ObservableProperty]
    public partial string PortraitFile { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string ItemId { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string QuantityText { get; set; } = "1";

    /// <summary>The object the event's object turns into.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string ObjectId { get; set; } = "";

    /// <summary>The map warped to.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string MapId { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string CellXText { get; set; } = "0";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string CellYText { get; set; } = "0";

    public string Label => Kind switch
    {
        EventStepKind.Dialogue => DialogueLabel(Speech()),
        EventStepKind.GiveItem => $"Give item: {ItemId} x{QuantityText}",
        EventStepKind.SwapObject => $"Swap object to: {ObjectId}",
        EventStepKind.Warp => $"Warp to: {MapId} ({CellXText}, {CellYText})",
        _ => throw new UnreachableException(),
    };

    /// <summary>Why the step can't be kept as typed; null when it can.</summary>
    public string? Problem => Kind switch
    {
        // A list the step's id isn't in can clear its choice, so empty covers null too.
        EventStepKind.GiveItem when string.IsNullOrEmpty(ItemId) => "Choose the item to give.",
        EventStepKind.SwapObject when string.IsNullOrEmpty(ObjectId) => "Choose the object it becomes.",
        EventStepKind.Warp when string.IsNullOrEmpty(MapId) => "Choose the map to warp to.",
        EventStepKind.GiveItem when WholeNumber(QuantityText, minimum: 1) is null => "The quantity must be a whole number of at least 1.",
        EventStepKind.Warp when WholeNumber(CellXText, minimum: 0) is null || WholeNumber(CellYText, minimum: 0) is null =>
            "The warp cell must be two whole numbers of at least 0.",
        _ => null,
    };

    public static EventStepViewModel From(EventStep step) => step switch
    {
        DialogueStep dialogue => new(EventStepKind.Dialogue)
        {
            LinesText = string.Join(Environment.NewLine, dialogue.Speech.Lines),
            Speaker = dialogue.Speech.Speaker ?? "",
            PortraitFile = dialogue.Speech.PortraitFile ?? "",
        },
        GiveItemStep give => new(EventStepKind.GiveItem) { ItemId = give.ItemId, QuantityText = Text(give.Quantity) },
        SwapObjectStep swap => new(EventStepKind.SwapObject) { ObjectId = swap.ObjectId },
        WarpStep warp => new(EventStepKind.Warp) { MapId = warp.MapId, CellXText = Text(warp.Cell.X), CellYText = Text(warp.Cell.Y) },
        _ => throw new ArgumentException($"Unknown step type {step.GetType().Name}.", nameof(step)),
    };

    /// <summary>A step of a kind with its fields empty, or 1 and (0, 0) for the numbers.</summary>
    public static EventStepViewModel New(EventStepKind kind) => new(kind);

    /// <summary>The step as edited, once <see cref="Problem"/> is null.</summary>
    public EventStep ToStep() => Kind switch
    {
        EventStepKind.Dialogue => new DialogueStep(Speech()),
        EventStepKind.GiveItem => new GiveItemStep(ItemId, WholeNumber(QuantityText, minimum: 1) ?? throw InvalidFields()),
        EventStepKind.SwapObject => new SwapObjectStep(ObjectId),
        EventStepKind.Warp => new WarpStep(MapId, new GridCell(
            WholeNumber(CellXText, minimum: 0) ?? throw InvalidFields(),
            WholeNumber(CellYText, minimum: 0) ?? throw InvalidFields())),
        _ => throw new UnreachableException(),
    };

    private Speech Speech() => new(
        [.. LinesText.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0)],
        string.IsNullOrWhiteSpace(Speaker) ? null : Speaker.Trim(),
        string.IsNullOrWhiteSpace(PortraitFile) ? null : PortraitFile.Trim());

    private InvalidOperationException InvalidFields() => new(Problem);

    private static string Text(int number) => number.ToString(CultureInfo.InvariantCulture);

    // Digits only.
    private static int? WholeNumber(string text, int minimum) =>
        int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number >= minimum ? number : null;

    private static string DialogueLabel(Speech speech)
    {
        var first = speech.Lines.Count > 0 ? speech.Lines[0] : "";
        var shown = first.Length > LabelLineLength ? $"{first[..LabelLineLength]}..." : first;
        var speaker = speech.Speaker is null ? "" : $"{speech.Speaker}: ";
        var more = speech.Lines.Count > 1 ? $" (+{speech.Lines.Count - 1})" : "";
        return $"Dialogue: {speaker}\"{shown}\"{more}";
    }
}
