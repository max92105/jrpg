using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Cinematics;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// The Cinematics page's Play (decision 620): a cinematic on the preview's own clock, the still at each moment faded,
/// zoomed about its focus and subtitled by Core's rules (decisions 629, 751), as the game shows it; silent, the editor
/// plays no audio. Stopped, it shows nothing of its own, and the page shows the chosen still.
/// </summary>
internal sealed partial class CinematicPreview : ObservableObject
{
    private CinematicDefinition? _cinematic;
    private TimeSpan _elapsed;

    [ObservableProperty]
    public partial bool IsPlaying { get; private set; }

    /// <summary>The picture of the still at the clock's moment; null while stopped.</summary>
    [ObservableProperty]
    public partial string? ShownImage { get; private set; }

    /// <summary>0 to 1, by Core's fade; 1 while stopped.</summary>
    [ObservableProperty]
    public partial double Opacity { get; private set; } = 1;

    /// <summary>The picture's size as a factor of its fitted size, by Core's zoom; 1 while stopped.</summary>
    [ObservableProperty]
    public partial double Scale { get; private set; } = 1;

    /// <summary>
    /// The point of the frame, as shares of its width and height, that Play's picture zooms about, by Core's
    /// <see cref="CinematicStill.ZoomOrigin"/>; the center while stopped.
    /// </summary>
    [ObservableProperty]
    public partial Point ZoomOrigin { get; private set; } = new(0.5, 0.5);

    /// <summary>The shown still's subtitle; null while stopped or when it has none.</summary>
    [ObservableProperty]
    public partial string? Caption { get; private set; }

    /// <summary>"Still 3 of 6, 2.4 s": which still shows and how far into it; empty while stopped.</summary>
    [ObservableProperty]
    public partial string TimeLine { get; private set; } = "";

    /// <summary>Plays the cinematic from the start of one of its stills.</summary>
    /// <param name="cinematic">The cinematic as typed, built without a problem.</param>
    /// <param name="fromStill">The still to start at, from 0.</param>
    public void Play(CinematicDefinition cinematic, int fromStill)
    {
        _cinematic = cinematic;
        _elapsed = TimeSpan.Zero;
        for (var index = 0; index < fromStill; index++)
        {
            _elapsed += cinematic.Stills[index].Duration;
        }

        IsPlaying = true;
        ShowMoment();
    }

    /// <summary>Moves the clock; past the last still's end, the preview stops.</summary>
    public void Advance(TimeSpan delta)
    {
        if (!IsPlaying)
        {
            return;
        }

        _elapsed += delta;
        ShowMoment();
    }

    public void Stop()
    {
        _cinematic = null;
        IsPlaying = false;
        ShownImage = null;
        Opacity = 1;
        Scale = 1;
        ZoomOrigin = new Point(0.5, 0.5);
        Caption = null;
        TimeLine = "";
    }

    private void ShowMoment()
    {
        if (_cinematic!.MomentAt(_elapsed) is not { } moment)
        {
            Stop();
            return;
        }

        var still = _cinematic.Stills[moment.StillIndex];
        ShownImage = still.ImageFile;
        Opacity = StillFade.OpacityAt(moment.IntoStill, still.Duration);
        Scale = still.ScaleAt(moment.IntoStill);
        ZoomOrigin = new Point(still.ZoomOrigin.X, still.ZoomOrigin.Y);
        Caption = still.Caption;
        TimeLine = string.Create(
            CultureInfo.InvariantCulture,
            $"Still {moment.StillIndex + 1} of {_cinematic.Stills.Count}, {moment.IntoStill.TotalSeconds:0.0} s");
    }
}
