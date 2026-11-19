using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Cinematics;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// The still's subtitles: a sequence of lines, each with its seconds, shown one after the other from the still's start
/// so they follow its voice. Add puts a line last, on the time the still has left; a line or its seconds typed, a line
/// added or removed, is the still's change.
/// </summary>
internal sealed partial class CinematicStillRow
{
    // A line added to a still whose time is already taken: about one short spoken sentence.
    private const double NewCaptionSeconds = 2;

    /// <summary>The subtitles in the order they show.</summary>
    public ObservableCollection<StillCaptionRow> Captions { get; } = [];

    /// <summary>
    /// "Subtitles: 3.5 s of 4 s", so the lines can be timed within the still; empty without a subtitle or while a box
    /// can't be read.
    /// </summary>
    public string CaptionsTimeText =>
        Captions.Count > 0 && SecondsOf(SecondsText) is { } seconds && Captions.All(caption => caption.Seconds is not null)
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"Subtitles: {Captions.Sum(caption => caption.Seconds!.Value):0.##} s of {seconds:0.##} s")
            : "";

    // The first subtitle, in the list's order, whose seconds can't be read.
    private string? CaptionProblem
    {
        get
        {
            for (var index = 0; index < Captions.Count; index++)
            {
                if (Captions[index].Seconds is null)
                {
                    var typed = Captions[index].SecondsText.Trim();
                    return $"Still {Number}: subtitle {index + 1}'s seconds must be a number above 0 (was '{typed}').";
                }
            }

            return null;
        }
    }

    [RelayCommand]
    private void AddCaption()
    {
        var taken = Captions.Sum(caption => caption.Seconds ?? 0);
        var left = SecondsOf(SecondsText) is { } seconds && seconds > taken ? seconds - taken : NewCaptionSeconds;
        Insert(new StillCaptionRow("", left.ToString("0.##", CultureInfo.InvariantCulture)));
        OnCaptionsChanged();
    }

    [RelayCommand]
    private void RemoveCaption(StillCaptionRow caption)
    {
        caption.Changed -= OnCaptionChanged;
        Captions.Remove(caption);
        OnCaptionsChanged();
    }

    private void ReadCaptions(IReadOnlyList<StillCaption> captions)
    {
        foreach (var caption in captions)
        {
            Insert(new StillCaptionRow(caption.Text, caption.Seconds.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private void Insert(StillCaptionRow caption)
    {
        caption.Changed += OnCaptionChanged;
        Captions.Add(caption);
    }

    private void OnCaptionChanged(object? sender, EventArgs e) => OnCaptionsChanged();

    private void OnCaptionsChanged()
    {
        OnPropertyChanged(nameof(CaptionsTimeText));
        OnPropertyChanged(nameof(Problem));
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
