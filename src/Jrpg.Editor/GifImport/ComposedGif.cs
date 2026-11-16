using System.IO;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// A GIF's whole frames at its own size and its most common frame time (decision 632), read once for the import
/// dialog's preview (decision 987), composed as the conversion composes them. WPF imaging needs an STA thread.
/// </summary>
/// <param name="Frames">Whole frames, BGRA packed as <see cref="GifFrame.Pixels"/>; one or more.</param>
/// <param name="Width">The canvas's width in pixels, each frame's, at least 1.</param>
/// <param name="Height">The canvas's height in pixels, each frame's, at least 1.</param>
/// <param name="FrameMilliseconds">The loop's frame time had nothing been typed.</param>
internal sealed record ComposedGif(IReadOnlyList<uint[]> Frames, int Width, int Height, int FrameMilliseconds)
{
    /// <summary>
    /// A one-frame GIF is read as any other: the conversion is what refuses it, naming it among the import's refusals.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// WPF can't decode the file as a GIF, or its canvas has no width or height, leaving no pixel to show; either
    /// message names the file.
    /// </exception>
    public static ComposedGif Read(string path)
    {
        var image = GifReader.Read(path);
        if (image.Width == 0 || image.Height == 0)
        {
            var problem = $"{Path.GetFileName(path)} has an empty canvas ({image.Width} x {image.Height} px).";
            throw new InvalidDataException(problem);
        }

        return new ComposedGif(
            GifFrameComposer.Compose(image), image.Width, image.Height, GifFrameTiming.FrameMilliseconds(image.Frames));
    }
}
