namespace Jrpg.Editor.GifImport;

/// <summary>
/// Makes a solid background transparent (decision 638), for a GIF drawn on one: the first frame's top-left color,
/// matched exactly, cleared in every frame.
/// </summary>
internal static class BackgroundRemoval
{
    /// <param name="frames">At least one whole frame, BGRA packed as <see cref="GifFrame.Pixels"/>.</param>
    /// <returns>New frames; the same frames when the top-left pixel is already transparent.</returns>
    public static IReadOnlyList<uint[]> Apply(IReadOnlyList<uint[]> frames)
    {
        var background = frames[0][0];
        if (background >> 24 == 0)
        {
            return frames;
        }

        return [.. frames.Select(frame => frame.Select(pixel => pixel == background ? 0 : pixel).ToArray())];
    }
}
