using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// A still's focus as the Cinematics page edits it (decisions 749, 750, 753, 754): kept from the record and written
// back, set from shares of the preview frame as whole percents clamped to the picture, cleared, named in a line, and
// counted as a typed change. On a still zooming 100 to 120 % toward 30 %, 62 %, built here.
public sealed class CinematicStillRowFocusTests
{
    private static readonly StillFocus _focus = new() { XPercent = 30, YPercent = 62 };

    private static readonly CinematicStill _focused = new()
    {
        ImageFile = "intro/1.png",
        Seconds = 4,
        Zoom = new StillZoom { StartPercent = 100, EndPercent = 120 },
        Focus = _focus,
    };

    [Fact]
    public void From_StillWithFocus_KeepsIt()
    {
        Assert.Equal(_focus, CinematicStillRow.From(_focused).Focus);
    }

    [Fact]
    public void ToStill_WithFocus_CarriesIt()
    {
        Assert.Equal(_focus, CinematicStillRow.From(_focused).ToStill().Focus);
    }

    [Fact]
    public void ToStill_NoFocus_IsNull()
    {
        Assert.Null(CinematicStillRow.From(_focused with { Focus = null }).ToStill().Focus);
    }

    // Decision 750: typing the zoom back to 100 and up again doesn't lose the click.
    [Fact]
    public void ToStill_FocusWithoutZoom_KeepsIt()
    {
        var row = CinematicStillRow.From(_focused);
        row.ZoomEndText = "100";

        var still = row.ToStill();

        Assert.Equal((null, _focus), (still.Zoom, still.Focus));
    }

    [Fact]
    public void SetFocusAt_SharesOfTheFrame_RoundsToWholePercents()
    {
        var row = CinematicStillRow.From(_focused);

        row.SetFocusAt(0.324, 0.616);

        Assert.Equal(new StillFocus { XPercent = 32, YPercent = 62 }, row.Focus);
    }

    // 1.01 rounds to 101, so the upper clamp is needed; 1.0008 is a click a pixel past the 480 x 270 frame's bottom.
    [Theory]
    [InlineData(-0.01, 1.01, 0, 100)]
    [InlineData(1.2, -3, 100, 0)]
    [InlineData(0.5, 1.0008, 50, 100)]
    public void SetFocusAt_PastTheEdges_ClampsToTheBounds(double xShare, double yShare, int xPercent, int yPercent)
    {
        var row = CinematicStillRow.From(_focused);

        row.SetFocusAt(xShare, yShare);

        Assert.Equal(new StillFocus { XPercent = xPercent, YPercent = yPercent }, row.Focus);
    }

    [Fact]
    public void SetFocusAt_OnARow_RaisesChanged()
    {
        var row = CinematicStillRow.From(_focused with { Focus = null });
        var changed = 0;
        row.Changed += (_, _) => changed++;

        row.SetFocusAt(0.25, 0.75);

        Assert.Equal(1, changed);
    }

    [Fact]
    public void ClearFocus_WithFocus_RemovesIt()
    {
        var row = CinematicStillRow.From(_focused);

        row.ClearFocusCommand.Execute(null);

        Assert.Null(row.Focus);
        Assert.Equal("Zoom heads to the center: click the picture to choose a point", row.FocusText);
    }

    [Fact]
    public void ClearFocusCommand_NoFocus_IsGreyed()
    {
        Assert.False(CinematicStillRow.From(_focused with { Focus = null }).ClearFocusCommand.CanExecute(null));
    }

    [Fact]
    public void ClearFocusCommand_FocusSet_RaisesCanExecuteChanged()
    {
        var row = CinematicStillRow.From(_focused with { Focus = null });
        var lit = false;
        row.ClearFocusCommand.CanExecuteChanged += (_, _) => lit = row.ClearFocusCommand.CanExecute(null);

        row.SetFocusAt(0.25, 0.75);

        Assert.True(lit);
    }

    [Fact]
    public void FocusText_WithFocus_NamesThePercents()
    {
        Assert.Equal("Zoom heads to 30 %, 62 %", CinematicStillRow.From(_focused).FocusText);
    }

    [Fact]
    public void FocusText_FocusSet_RaisesPropertyChanged()
    {
        var row = CinematicStillRow.From(_focused with { Focus = null });
        var raised = new List<string?>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        row.SetFocusAt(0.25, 0.75);

        Assert.Contains(nameof(CinematicStillRow.FocusText), raised);
    }
}
