using System.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// What the Effect strips window's preview plays and says, on a four-frame image of 8 x 8 frames looked up by the test.
public sealed class EffectStripPreviewTests
{
    private const string Burst = "Burst.png";

    [Fact]
    public void PreviewLoop_FourFramesTyped60_IsTheTypedPace()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "60";

        Assert.Equal(new TileAnimation { FrameCount = 4, FrameMilliseconds = 60 }, strip.PreviewLoop);
    }

    [Fact]
    public void PreviewLoop_FrameTimeOne_IsAPace()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "1";

        Assert.Equal(1, strip.PreviewLoop?.FrameMilliseconds);
    }

    [Fact]
    public void PreviewLoop_FrameTimeZero_IsNone()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "0";

        Assert.Null(strip.PreviewLoop);
    }

    [Fact]
    public void PreviewLoop_FrameTimeNotANumber_IsNone()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "fast";

        Assert.Null(strip.PreviewLoop);
    }

    [Fact]
    public void PreviewLoop_ImageMissing_IsNone()
    {
        Assert.Null(EffectStripViewModel.Unlisted("Gone.png", SizeOf).PreviewLoop);
    }

    [Fact]
    public void PreviewLoop_ImageTallerThanWide_IsNone()
    {
        Assert.Null(EffectStripViewModel.Unlisted("Tall.png", SizeOf).PreviewLoop);
    }

    [Fact]
    public void FrameMillisecondsText_Changed_RaisesPreviewLoop()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);
        var notified = new List<string?>();
        ((INotifyPropertyChanged)strip).PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        strip.FrameMillisecondsText = "60";

        Assert.Contains(nameof(EffectStripViewModel.PreviewLoop), notified);
    }

    [Fact]
    public void FrameLineAt_TheHitFrame_SaysTheHit()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "2";

        Assert.Equal("frame 3 of 4, the hit", strip.FrameLineAt(2));
    }

    [Fact]
    public void FrameLineAt_AnotherFrame_IsTheFrameOnly()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "2";

        Assert.Equal("frame 2 of 4", strip.FrameLineAt(1));
    }

    // The test's effects folder: Burst.png is four 8 x 8 frames, Tall.png narrower than high, and the rest missing.
    private static (int Width, int Height)? SizeOf(string file) => file switch
    {
        Burst => (32, 8),
        "Tall.png" => (8, 16),
        _ => null,
    };
}
