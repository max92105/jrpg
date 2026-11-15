namespace Jrpg.Editor.GifImport;

/// <summary>
/// One frame time for a loop from a GIF's delays (decision 632): the most common, the first of equals in frame order. A
/// delay under 20 ms reads as 100 ms, as browsers play a GIF's 0 and 10 ms.
/// </summary>
internal static class GifFrameTiming
{
    private const int ShortestPlayedMilliseconds = 20;
    private const int ShortDelayPlayedAs = 100;
    private const int MillisecondsPerCentisecond = 10;

    /// <exception cref="InvalidOperationException">No frame.</exception>
    public static int FrameMilliseconds(IReadOnlyList<GifFrame> frames) =>
        frames.Select(PlayedMilliseconds).GroupBy(time => time).OrderByDescending(times => times.Count()).First().Key;

    /// <summary>Whether every frame plays as long as the others, so the one frame time loses nothing.</summary>
    public static bool AreDelaysEqual(IReadOnlyList<GifFrame> frames) =>
        frames.Select(PlayedMilliseconds).Distinct().Count() <= 1;

    private static int PlayedMilliseconds(GifFrame frame)
    {
        var stored = frame.DelayCentiseconds * MillisecondsPerCentisecond;
        return stored < ShortestPlayedMilliseconds ? ShortDelayPlayedAs : stored;
    }
}
