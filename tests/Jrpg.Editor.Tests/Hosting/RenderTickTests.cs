using Jrpg.Editor.Hosting;

namespace Jrpg.Editor.Tests.Hosting;

// A window's per-frame work stopped when it throws (ADR 0050), its frames raised by the test: no render pass needed.
public sealed class RenderTickTests
{
    [Fact]
    public void OnFrame_WorkRuns_ReportsNothing()
    {
        var (frames, reported) = (0, new List<Exception>());
        var tick = new RenderTick((_, _) => frames++, reported.Add);

        tick.OnFrame(null, EventArgs.Empty);

        Assert.Equal((1, 0), (frames, reported.Count));
    }

    [Fact]
    public void OnFrame_WorkThrows_ReportsItsException()
    {
        var failure = new InvalidOperationException("The drawing failed.");
        var reported = new List<Exception>();
        var tick = new RenderTick((_, _) => throw failure, reported.Add);

        tick.OnFrame(null, EventArgs.Empty);

        Assert.Equal([failure], reported);
    }

    // On the render event itself, on a thread WPF can draw from: after the throw the tick no longer runs.
    [Fact]
    public void OnFrame_WorkThrowsWhileRunning_StopsTheTick()
    {
        var (ranBefore, runsAfter) = (false, true);

        StaThread.Run(() =>
        {
            var tick = new RenderTick((_, _) => throw new InvalidOperationException("The drawing failed."), _ => { });
            tick.Start();
            ranBefore = tick.IsRunning;

            tick.OnFrame(null, EventArgs.Empty);

            runsAfter = tick.IsRunning;
        });

        Assert.Equal((true, false), (ranBefore, runsAfter));
    }

    [Fact]
    public void Stop_RunningTick_NoLongerRuns()
    {
        var runsAfter = true;

        StaThread.Run(() =>
        {
            var tick = new RenderTick((_, _) => { }, _ => { });
            tick.Start();

            tick.Stop();

            runsAfter = tick.IsRunning;
        });

        Assert.False(runsAfter);
    }
}
