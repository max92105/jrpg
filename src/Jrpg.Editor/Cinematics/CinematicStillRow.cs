using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Cinematics;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// One still as the Cinematics page edits it (ADR 0046): its picture, seconds, subtitle, voice and zoom boxes kept as
/// typed, and the point its zoom heads to, set by a click on the picture. <see cref="Problem"/> is only a box that
/// can't be read; the ranges are Data's rules, which the page's problem line shows for the whole list.
/// </summary>
internal sealed partial class CinematicStillRow : ObservableObject
{
    // A new still's boxes (decision 616): the four seconds today's stills show, and no zoom.
    private const double NewSeconds = 4;

    // A dot decimal, or the exponent form a tiny time is shown in ("1E-05").
    private const NumberStyles SecondsStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

    private CinematicStillRow()
    {
    }

    /// <summary>A typed box changed: what the list's problem line listens to, never the row's own label.</summary>
    public event EventHandler? Changed;

    /// <summary>The still's place from 1, as the page numbers the rows; the list sets it.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial int Number { get; set; }

    /// <summary>The picture under the cinematics art folder, its cinematic's folder included (decision 610).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string ImageFile { get; set; } = "";

    /// <summary>How long the still shows, as typed: a dot decimal.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem), nameof(IsShorterThanVoice))]
    public partial string SecondsText { get; set; } = "";

    /// <summary>The subtitle; empty for none.</summary>
    [ObservableProperty]
    public partial string Caption { get; set; } = "";

    /// <summary>The voice line in the voices folder; empty for none.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VoiceLengthText), nameof(IsShorterThanVoice))]
    public partial string VoiceFile { get; set; } = "";

    /// <summary>
    /// A voice line's playing length by its name in the voices folder, null when it can't be read (decision 620); the
    /// list sets it from the page's choices, as it sets <see cref="Number"/>. No length is known until then.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VoiceLengthText), nameof(IsShorterThanVoice))]
    public partial Func<string, TimeSpan?> VoiceLengthOf { get; set; } = _ => null;

    /// <summary>The zoom's start in percent, as typed; 100 at both ends is no zoom (decision 617).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ZoomStartText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ZoomEndText { get; set; } = "";

    /// <summary>
    /// Where the zoom heads, null for the center (decision 746): set by a click, never typed, so never a problem; kept
    /// without a zoom (decision 750).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFocus), nameof(FocusText))]
    [NotifyCanExecuteChangedFor(nameof(ClearFocusCommand))]
    public partial StillFocus? Focus { get; private set; }

    public bool HasFocus => Focus is not null;

    /// <summary>The line under the zoom boxes (decision 753): the point's percents, or how to choose one.</summary>
    public string FocusText => Focus is { } focus
        ? $"Zoom heads to {focus.XPercent} %, {focus.YPercent} %"
        : "Zoom heads to the center: click the picture to choose a point";

    /// <summary>
    /// "voice 5.3 s" beside the Seconds box, so the still can be timed to its voice without audio in the editor
    /// (decision 620); "voice not found" when the folder lacks it; empty without a voice.
    /// </summary>
    public string VoiceLengthText =>
        VoiceFile.Trim() is not { Length: > 0 } voice ? ""
        : VoiceLengthOf(voice) is { } length ? string.Create(CultureInfo.InvariantCulture, $"voice {ShownSeconds(length):0.0} s")
        : "voice not found";

    /// <summary>
    /// The still ends before its voice does, which the page shows in the warning color: the voice runs on over the next
    /// still by design (decision 613), the color only says so.
    /// </summary>
    public bool IsShorterThanVoice =>
        VoiceFile.Trim() is { Length: > 0 } voice
        && VoiceLengthOf(voice) is { } length
        && Seconds(SecondsText) is { } seconds
        && seconds < ShownSeconds(length);

    /// <summary>"3. ragnir_intro/3_son_becoming_soldier.png, 6 s".</summary>
    public string Label => $"{Number}. {ImageFile}, {SecondsText} s";

    /// <summary>The first box, in the row's order, that can't be read; null when the still can be built.</summary>
    public string? Problem =>
        string.IsNullOrWhiteSpace(ImageFile) ? $"Still {Number}: choose its picture."
        : Seconds(SecondsText) is null ? $"Still {Number}: the seconds must be a number above 0 (was '{SecondsText.Trim()}')."
        : (TypedNumber.ProblemOf("zoom start", ZoomStartText) ?? TypedNumber.ProblemOf("zoom end", ZoomEndText)) is { } zoom
            ? $"Still {Number}: {zoom}"
        : null;

    public static CinematicStillRow From(CinematicStill still) => new()
    {
        ImageFile = still.ImageFile,
        SecondsText = still.Seconds.ToString(CultureInfo.InvariantCulture),
        Caption = still.Caption ?? "",
        VoiceFile = still.VoiceFile ?? "",
        ZoomStartText = TypedNumber.Text(still.Zoom?.StartPercent ?? StillZoom.MinPercent),
        ZoomEndText = TypedNumber.Text(still.Zoom?.EndPercent ?? StillZoom.MinPercent),
        Focus = still.Focus,
    };

    /// <summary>A still just added on a picture: four seconds, no subtitle, voice or zoom (decision 616).</summary>
    public static CinematicStillRow New(string imageFile) =>
        From(new CinematicStill { ImageFile = imageFile, Seconds = NewSeconds });

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public CinematicStill ToStill()
    {
        if (Problem is { } problem)
        {
            throw new InvalidOperationException(problem);
        }

        return new CinematicStill
        {
            ImageFile = ImageFile.Trim(),
            Seconds = Seconds(SecondsText)!.Value,
            Caption = TextOrNull(Caption),
            VoiceFile = TextOrNull(VoiceFile),
            Zoom = ZoomOrNull(TypedNumber.Parse(ZoomStartText)!.Value, TypedNumber.Parse(ZoomEndText)!.Value),
            Focus = Focus,
        };
    }

    /// <summary>
    /// Sets the point where a click landed, as shares of the preview frame, which is the picture as the screen shows it
    /// (decision 747): each rounded to a whole percent and clamped to the picture, so a click a pixel past an edge is
    /// that edge (decision 749).
    /// </summary>
    public void SetFocusAt(double xShare, double yShare) =>
        Focus = new StillFocus { XPercent = PercentOf(xShare), YPercent = PercentOf(yShare) };

    partial void OnImageFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnSecondsTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnCaptionChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnVoiceFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnZoomStartTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnZoomEndTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    // A typed change too (decision 754): the list's relay stops Play and refreshes the problem line.
    partial void OnFocusChanged(StillFocus? value) => Changed?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(HasFocus))]
    private void ClearFocus() => Focus = null;

    private static int PercentOf(double share) =>
        Math.Clamp((int)Math.Round(share * 100, MidpointRounding.AwayFromZero), StillFocus.MinPercent, StillFocus.MaxPercent);

    // 100 at both ends draws the still at its fitted size, which content writes as no zoom (decision 617).
    private static StillZoom? ZoomOrNull(int startPercent, int endPercent) =>
        startPercent == StillZoom.MinPercent && endPercent == StillZoom.MinPercent
            ? null
            : new StillZoom { StartPercent = startPercent, EndPercent = endPercent };

    // Up to a tenth, so typing the length shown never shows the warning.
    private static double ShownSeconds(TimeSpan length) => Math.Ceiling(length.TotalSeconds * 10) / 10;

    private static string? TextOrNull(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    // TryParse also reads "Infinity", which content can't hold.
    private static double? Seconds(string text) =>
        double.TryParse(text.Trim(), SecondsStyle, CultureInfo.InvariantCulture, out var seconds)
        && seconds > 0
        && double.IsFinite(seconds)
            ? seconds
            : null;
}
