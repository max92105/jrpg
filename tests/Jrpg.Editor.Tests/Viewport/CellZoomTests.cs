using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Tests.Viewport;

public sealed class CellZoomTests
{
    private const int MapCellPixels = 48;

    [Fact]
    public void Constructor_StartNotAStep_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => new CellZoom(50));

    [Fact]
    public void ZoomIn_AtTheMapSize_ShowsSixtyFour()
    {
        var zoom = new CellZoom(MapCellPixels);

        zoom.ZoomInCommand.Execute(null);

        Assert.Equal(64, zoom.CellPixels);
    }

    [Fact]
    public void ZoomOut_AtTheMapSize_ShowsThirtyTwo()
    {
        var zoom = new CellZoom(MapCellPixels);

        zoom.ZoomOutCommand.Execute(null);

        Assert.Equal(32, zoom.CellPixels);
    }

    [Fact]
    public void ZoomInCommand_AtTheLargestStep_CannotExecute() => Assert.False(new CellZoom(192).ZoomInCommand.CanExecute(null));

    [Fact]
    public void ZoomInCommand_OneStepBelowTheLargest_CanExecute() => Assert.True(new CellZoom(128).ZoomInCommand.CanExecute(null));

    [Fact]
    public void ZoomOutCommand_AtTheSmallestStep_CannotExecute() => Assert.False(new CellZoom(16).ZoomOutCommand.CanExecute(null));

    [Fact]
    public void ZoomOutCommand_OneStepAboveTheSmallest_CanExecute() => Assert.True(new CellZoom(32).ZoomOutCommand.CanExecute(null));

    [Fact]
    public void ZoomInCommand_ReachingTheLargestStep_RaisesCanExecuteChanged()
    {
        var zoom = new CellZoom(128);
        var isRaised = false;
        zoom.ZoomInCommand.CanExecuteChanged += (_, _) => isRaised = true;

        zoom.ZoomInCommand.Execute(null);

        Assert.True(isRaised);
        Assert.False(zoom.ZoomInCommand.CanExecute(null));
    }

    [Fact]
    public void ZoomOutCommand_ReachingTheSmallestStep_RaisesCanExecuteChanged()
    {
        var zoom = new CellZoom(32);
        var isRaised = false;
        zoom.ZoomOutCommand.CanExecuteChanged += (_, _) => isRaised = true;

        zoom.ZoomOutCommand.Execute(null);

        Assert.True(isRaised);
        Assert.False(zoom.ZoomOutCommand.CanExecute(null));
    }

    [Fact]
    public void Reset_AfterZoomingIn_ShowsTheStartSize()
    {
        var zoom = new CellZoom(MapCellPixels);
        zoom.ZoomInCommand.Execute(null);

        zoom.ResetCommand.Execute(null);

        Assert.Equal(MapCellPixels, zoom.CellPixels);
    }

    [Fact]
    public void CellPixelsText_AtTheMapSize_IsFortyEightPixels() => Assert.Equal("48 px", new CellZoom(MapCellPixels).CellPixelsText);

    [Fact]
    public void CellPixelsText_AfterZoomingIn_IsRaised()
    {
        var zoom = new CellZoom(MapCellPixels);
        var raised = new List<string?>();
        zoom.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        zoom.ZoomInCommand.Execute(null);

        Assert.Contains(nameof(CellZoom.CellPixelsText), raised);
        Assert.Equal("64 px", zoom.CellPixelsText);
    }

    [Fact]
    public void ScaleFor_SixteenPixelSourceAtTheMapSize_IsThree() => Assert.Equal(3, new CellZoom(MapCellPixels).ScaleFor(16));

    [Theory]
    [MemberData(nameof(EveryStep))]
    public void ScaleFor_SixteenPixelSourceAtEveryStep_IsAWholeNumber(int step)
    {
        var scale = new CellZoom(step).ScaleFor(16);

        Assert.Equal(Math.Round(scale), scale);
    }

    [Fact]
    public void OffsetKeeping_DoublingTheScale_KeepsTheContentPointUnderTheAnchor() =>
        Assert.Equal(250, CellZoom.OffsetKeeping(offset: 100, anchor: 50, oldScale: 3, newScale: 6));

    [Fact]
    public void OffsetKeeping_ZoomingOutNearTheTopLeft_IsNeverNegative() =>
        Assert.Equal(0, CellZoom.OffsetKeeping(offset: 0, anchor: 200, oldScale: 6, newScale: 3));

    public static TheoryData<int> EveryStep() => [.. CellZoom.Steps];
}
