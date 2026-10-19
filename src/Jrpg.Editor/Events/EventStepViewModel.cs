using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;

namespace Jrpg.Editor.Events;

/// <summary>
/// One step in the event dialog: its fields and its one-line label (legacy <c>Describe</c>). Dialogue steps are edited
/// here; the other kinds keep the step as it was until their fields come (plan slice 11i-2).
/// </summary>
internal sealed partial class EventStepViewModel : ObservableObject
{
    // The legacy label's limit on a dialogue's first line.
    private const int LabelLineLength = 30;

    // A step that can't be edited yet, kept unchanged; null for a dialogue.
    private readonly EventStep? _keptStep;

    private EventStepViewModel(EventStep? keptStep, Speech speech)
    {
        _keptStep = keptStep;
        LinesText = string.Join(Environment.NewLine, speech.Lines);
        Speaker = speech.Speaker ?? "";
        PortraitFile = speech.PortraitFile ?? "";
    }

    public bool IsDialogue => _keptStep is null;

    /// <summary>One message box per line; blank lines are left out.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string LinesText { get; set; }

    /// <summary>Empty for no speaker name.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Speaker { get; set; }

    /// <summary>The portrait's file in the portraits folder; empty for none.</summary>
    [ObservableProperty]
    public partial string PortraitFile { get; set; }

    public string Label => (_keptStep ?? ToDialogue()) switch
    {
        DialogueStep dialogue => DialogueLabel(dialogue.Speech),
        GiveItemStep give => $"Give item: {give.ItemId} x{give.Quantity}",
        SwapObjectStep swap => $"Swap object to: {swap.ObjectId}",
        WarpStep warp => $"Warp to: {warp.MapId} ({warp.Cell.X}, {warp.Cell.Y})",
        var other => other.GetType().Name,
    };

    public static EventStepViewModel From(EventStep step) =>
        step is DialogueStep dialogue ? new(null, dialogue.Speech) : new(step, new Speech([]));

    public static EventStepViewModel NewDialogue() => new(null, new Speech([]));

    public EventStep ToStep() => _keptStep ?? ToDialogue();

    private DialogueStep ToDialogue() => new(new Speech(
        [.. LinesText.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0)],
        string.IsNullOrWhiteSpace(Speaker) ? null : Speaker.Trim(),
        string.IsNullOrWhiteSpace(PortraitFile) ? null : PortraitFile.Trim()));

    private static string DialogueLabel(Speech speech)
    {
        var first = speech.Lines.Count > 0 ? speech.Lines[0] : "";
        var shown = first.Length > LabelLineLength ? $"{first[..LabelLineLength]}..." : first;
        var speaker = speech.Speaker is null ? "" : $"{speech.Speaker}: ";
        var more = speech.Lines.Count > 1 ? $" (+{speech.Lines.Count - 1})" : "";
        return $"Dialogue: {speaker}\"{shown}\"{more}";
    }
}
