using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Cinematics;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// One subtitle of a still's sequence as the Cinematics page edits it: its line and its seconds kept as typed. An empty
/// line is a pause, shown for its time as nothing.
/// </summary>
internal sealed partial class StillCaptionRow : ObservableObject
{
    /// <param name="text">The line; empty for a pause.</param>
    /// <param name="secondsText">How long it shows, as typed: a dot decimal.</param>
    public StillCaptionRow(string text, string secondsText)
    {
        Text = text;
        SecondsText = secondsText;
    }

    /// <summary>A box was typed in: what the still's row relays to the list's problem line.</summary>
    public event EventHandler? Changed;

    [ObservableProperty]
    public partial string Text { get; set; }

    [ObservableProperty]
    public partial string SecondsText { get; set; }

    /// <summary>The typed seconds by the still's own rule; null while they aren't a number above 0.</summary>
    public double? Seconds => CinematicStillRow.SecondsOf(SecondsText);

    /// <exception cref="InvalidOperationException">The seconds can't be read.</exception>
    public StillCaption ToCaption() =>
        Seconds is { } seconds
            ? new StillCaption { Text = Text.Trim(), Seconds = seconds }
            : throw new InvalidOperationException($"The subtitle's seconds can't be read (was '{SecondsText}').");

    partial void OnTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnSecondsTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
