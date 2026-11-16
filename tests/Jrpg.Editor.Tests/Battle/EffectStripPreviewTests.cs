using System.ComponentModel;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The typed boxes as the Effect strips page's preview reads them, on a four-frame image looked up by the test.
public sealed class EffectStripPreviewTests
{
    private const string Burst = "Burst.png";

    [Fact]
    public void FrameMilliseconds_Typed_IsIt()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "60";

        Assert.Equal(60, strip.FrameMilliseconds);
    }

    [Fact]
    public void FrameMilliseconds_TypedNotANumber_IsNull()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "fast";

        Assert.Null(strip.FrameMilliseconds);
    }

    [Fact]
    public void HitFrame_Typed_IsIt()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "2";

        Assert.Equal(2, strip.HitFrame);
    }

    [Fact]
    public void HitFrame_TypedNotANumber_IsNull()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "x";

        Assert.Null(strip.HitFrame);
    }

    [Fact]
    public void FrameMillisecondsText_Changed_RaisesFrameMilliseconds()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);
        var notified = new List<string?>();
        ((INotifyPropertyChanged)strip).PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        strip.FrameMillisecondsText = "60";

        Assert.Contains(nameof(EffectStripViewModel.FrameMilliseconds), notified);
    }

    [Fact]
    public void HitFrameText_Changed_RaisesHitFrame()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);
        var notified = new List<string?>();
        ((INotifyPropertyChanged)strip).PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        strip.HitFrameText = "2";

        Assert.Contains(nameof(EffectStripViewModel.HitFrame), notified);
    }

    // The test's effects folder: Burst.png is four 8 x 8 frames.
    private static (int Width, int Height)? SizeOf(string file) => file == Burst ? (32, 8) : null;
}
