using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// Play in the Cinematics window (decision 620), on a 2 s still zooming 100 to 110 %, then a 3 s one, each subtitled:
// what shows at each moment of the preview's clock, by Core's fade and zoom.
public sealed class CinematicPreviewTests
{
    private static readonly CinematicDefinition _intro = new()
    {
        Id = "intro",
        Stills =
        [
            new CinematicStill
            {
                ImageFile = "intro/1.png",
                Seconds = 2,
                Caption = "Long ago.",
                Zoom = new StillZoom { StartPercent = 100, EndPercent = 110 },
            },
            new CinematicStill { ImageFile = "intro/2.png", Seconds = 3, Caption = "Far away." },
        ],
    };

    private readonly CinematicPreview _preview = new();

    [Fact]
    public void Play_FromStillTwo_StartsAtItsBeginning()
    {
        _preview.Play(_intro, fromStill: 1);

        Assert.Equal(("intro/2.png", "Far away."), (_preview.ShownImage, _preview.Caption));
        Assert.Equal(0, _preview.Opacity);
    }

    [Fact]
    public void Advance_IntoTheFirstStill_FadesItIn()
    {
        _preview.Play(_intro, fromStill: 0);

        _preview.Advance(TimeSpan.FromSeconds(0.2));

        Assert.Equal(StillFade.OpacityAt(TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(2)), _preview.Opacity, precision: 6);
        Assert.True(_preview.Opacity is > 0 and < 1);
    }

    [Fact]
    public void Advance_HalfwayThroughTheFirst_ScalesItByItsZoom()
    {
        _preview.Play(_intro, fromStill: 0);

        _preview.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(1.05, _preview.Scale, precision: 6);
    }

    [Fact]
    public void Advance_PastTheEnd_Stops()
    {
        _preview.Play(_intro, fromStill: 0);

        _preview.Advance(TimeSpan.FromSeconds(5));

        Assert.False(_preview.IsPlaying);
        Assert.Null(_preview.ShownImage);
    }

    [Fact]
    public void Stop_WhilePlaying_ShowsTheChosenStillAtFullOpacity()
    {
        _preview.Play(_intro, fromStill: 0);
        _preview.Advance(TimeSpan.FromSeconds(1));

        _preview.Stop();

        Assert.False(_preview.IsPlaying);
        Assert.Equal((null, null, ""), (_preview.ShownImage, _preview.Caption, _preview.TimeLine));
        Assert.Equal((1.0, 1.0), (_preview.Opacity, _preview.Scale));
    }

    [Fact]
    public void TimeLine_InTheSecondStill_NamesItAndTheTimeInto()
    {
        _preview.Play(_intro, fromStill: 0);

        _preview.Advance(TimeSpan.FromSeconds(4.4));

        Assert.Equal("Still 2 of 2, 2.4 s", _preview.TimeLine);
    }

    [Fact]
    public void Advance_WhileStopped_DoesNothing()
    {
        _preview.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal((false, null), (_preview.IsPlaying, _preview.ShownImage));
    }
}
