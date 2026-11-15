using System.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Battle;

// One effect strip as typed (decision 659), on a four-frame image of 8 x 8 frames looked up by the test.
public sealed class EffectStripViewModelTests
{
    private const string Burst = "Burst.png";

    private static readonly EffectStripDefinition _listed =
        new() { File = Burst, FrameMilliseconds = 17, HitFrame = 2, SwingFile = "Swing.png" };

    [Fact]
    public void From_Record_ShowsItsBoxes()
    {
        var strip = EffectStripViewModel.From(_listed, SizeOf);

        Assert.Equal(("17", "2", "Swing.png"), (strip.FrameMillisecondsText, strip.HitFrameText, strip.SwingFile));
    }

    [Fact]
    public void Unlisted_ShowsTheUnlistedPace()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        Assert.Equal(("80", "0", ""), (strip.FrameMillisecondsText, strip.HitFrameText, strip.SwingFile));
    }

    [Fact]
    public void Unlisted_IsNotListed()
    {
        Assert.False(EffectStripViewModel.Unlisted(Burst, SizeOf).IsListed);
    }

    [Fact]
    public void IsListed_FrameTimeChanged_IsTrue()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "60";

        Assert.True(strip.IsListed);
    }

    [Fact]
    public void IsListed_BackToTheUnlistedPace_IsFalse()
    {
        var strip = EffectStripViewModel.From(_listed, SizeOf);

        strip.FrameMillisecondsText = "80";
        strip.HitFrameText = "0";
        strip.SwingFile = " ";

        Assert.False(strip.IsListed);
    }

    [Fact]
    public void LengthText_FourFramesAt80_SaysTheLengthAndTheHit()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "2";

        Assert.Equal("4 frames, 0.32 s; hit at 0.16 s", strip.LengthText);
    }

    [Fact]
    public void FrameCount_ImageMissing_IsNone()
    {
        var strip = EffectStripViewModel.Unlisted("Gone.png", SizeOf);

        Assert.Equal((null, "image missing"), (strip.FrameCount, strip.LengthText));
    }

    [Fact]
    public void Problem_FrameTimeNotANumber_NamesTheBox()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "fast";

        Assert.Equal("Frame time must be a whole number (was 'fast').", strip.Problem);
    }

    [Fact]
    public void Problem_HitFramePastTheFrames_NamesIt()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "4";

        Assert.Equal("Hit frame 4 is past the image's 4 frames (from 0).", strip.Problem);
    }

    [Fact]
    public void Problem_HitFrameOnTheLastFrame_IsNone()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "3";

        Assert.Null(strip.Problem);
    }

    [Fact]
    public void ToStrip_EmptySwing_IsNone()
    {
        var strip = EffectStripViewModel.From(_listed, SizeOf);

        strip.SwingFile = "  ";

        Assert.Null(strip.ToStrip().SwingFile);
    }

    [Fact]
    public void ToStrip_TypedSwing_IsTrimmed()
    {
        var strip = EffectStripViewModel.From(_listed with { SwingFile = null }, SizeOf);

        strip.SwingFile = " Sweep.png ";

        Assert.Equal(_listed with { SwingFile = "Sweep.png" }, strip.ToStrip());
    }

    [Fact]
    public void FrameMillisecondsText_Changed_RaisesProblemAndLength()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);
        var notified = new List<string?>();
        ((INotifyPropertyChanged)strip).PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        strip.FrameMillisecondsText = "60";

        Assert.Contains(nameof(EffectStripViewModel.Problem), notified);
        Assert.Contains(nameof(EffectStripViewModel.LengthText), notified);
    }

    [Fact]
    public void SwingFile_Chosen_RaisesIsListed()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);
        var notified = new List<string?>();
        ((INotifyPropertyChanged)strip).PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        strip.SwingFile = "Sweep.png";

        Assert.Contains(nameof(EffectStripViewModel.IsListed), notified);
    }

    [Fact]
    public void HitFrameText_Changed_RaisesProblemLengthAndIsListed()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);
        var notified = new List<string?>();
        ((INotifyPropertyChanged)strip).PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        strip.HitFrameText = "1";

        Assert.Contains(nameof(EffectStripViewModel.LengthText), notified);
        Assert.Contains(nameof(EffectStripViewModel.IsListed), notified);
        Assert.Contains(nameof(EffectStripViewModel.Problem), notified);
    }

    [Fact]
    public void IsListed_HitFrameChanged_IsTrue()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "2";

        Assert.True(strip.IsListed);
    }

    [Fact]
    public void IsListed_SwingChosen_IsTrue()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.SwingFile = "Sweep.png";

        Assert.True(strip.IsListed);
    }

    [Fact]
    public void IsListed_UnlistedFrameTimeWithALeadingZero_IsFalse()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = "080";

        Assert.False(strip.IsListed);
    }

    [Fact]
    public void LengthText_FrameTimePastAnIntOverTheStrip_DoesNotWrap()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.FrameMillisecondsText = TypedNumber.Text(int.MaxValue);

        Assert.Equal("4 frames, 8589934.59 s; hit at 0.00 s", strip.LengthText);
    }

    [Fact]
    public void ToStrip_HitFramePastTheFrames_Throws()
    {
        var strip = EffectStripViewModel.Unlisted(Burst, SizeOf);

        strip.HitFrameText = "4";

        Assert.Throws<InvalidOperationException>(strip.ToStrip);
    }

    // The test's effects folder: Burst.png is four 8 x 8 frames; anything else is missing.
    private static (int Width, int Height)? SizeOf(string file) => file == Burst ? (32, 8) : null;
}
