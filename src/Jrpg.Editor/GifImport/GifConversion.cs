using System.Globalization;
using System.IO;
using Jrpg.Core.Battle;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// One GIF into a strip (ADR 0047): read, made whole, its background cleared when asked, scaled to the chosen height,
/// and written as a PNG of the GIF's name into a fresh temporary folder. A battle sprite's strip carries its loop; an
/// attack effect's frames are each padded to a square and play at the effects' own pace, the GIF's time dropped
/// (decision 639). It runs on an STA thread, as WPF imaging needs.
/// </summary>
internal static class GifConversion
{
    /// <summary>What a strip's temporary folder starts with, under the system's temporary folder.</summary>
    public const string TemporaryFolderPrefix = "jrpg-gif-strip-";

    /// <exception cref="InvalidDataException">
    /// The GIF can't be read, or has one frame; the message names it.
    /// </exception>
    public static ConvertedGif Convert(string gifPath, GifImportOptions options)
    {
        var gif = Prepared(gifPath, options);
        var stripPath = Written(gifPath, path => FrameStrip.WritePng(gif.Frames, gif.FrameWidth, options.BattleHeight, path));
        var frameTime = GifFrameTiming.FrameMilliseconds(gif.Stored);
        var loop = new TileAnimation { FrameCount = gif.Frames.Count, FrameMilliseconds = frameTime };
        return new ConvertedGif(stripPath, loop, LoopNoteOf(loop, (gif.FrameWidth, options.BattleHeight), gif.Stored));
    }

    /// <exception cref="InvalidDataException">
    /// The GIF can't be read, or has one frame; the message names it.
    /// </exception>
    public static ConvertedGif ConvertToEffect(string gifPath, GifImportOptions options)
    {
        var gif = Prepared(gifPath, options);
        var stripPath = Written(
            gifPath, path => FrameStrip.WriteSquarePng(gif.Frames, gif.FrameWidth, options.BattleHeight, path));
        return new ConvertedGif(stripPath, null, EffectNoteOf(gif.Frames.Count, gif.Stored));
    }

    private static PreparedGif Prepared(string gifPath, GifImportOptions options)
    {
        var image = GifReader.Read(gifPath);
        if (image.Frames.Count < TileAnimation.MinFrames)
        {
            throw new InvalidDataException($"{Path.GetFileName(gifPath)} has one frame: import a PNG.");
        }

        var frames = GifFrameComposer.Compose(image);
        var cleared = options.RemovesBackground ? BackgroundRemoval.Apply(frames) : frames;
        var scaled = FrameScaler.Scale(cleared, image.Width, image.Height, options.BattleHeight);
        return new PreparedGif(scaled, FrameScaler.WidthAt(image.Width, image.Height, options.BattleHeight), image.Frames);
    }

    // The caller deletes the folder once it has the strip's path: a failed write deletes it here.
    private static string Written(string gifPath, Action<string> write)
    {
        var folder = Directory.CreateTempSubdirectory(TemporaryFolderPrefix).FullName;
        var stripPath = Path.Combine(folder, Path.ChangeExtension(Path.GetFileName(gifPath), ".png"));
        try
        {
            write(stripPath);
            return stripPath;
        }
        catch
        {
            Directory.Delete(folder, recursive: true);
            throw;
        }
    }

    private static string LoopNoteOf(TileAnimation loop, (int Width, int Height) frame, IReadOnlyList<GifFrame> stored)
    {
        var note = $"{loop.FrameCount} frames of {frame.Width} x {frame.Height} px, {loop.FrameMilliseconds} ms";
        return GifFrameTiming.AreDelaysEqual(stored)
            ? note
            : $"{note}; the GIF's frame times differed, {loop.FrameMilliseconds} ms is the most common";
    }

    // "12 frames, 0.96 s; effects play at 80 ms a frame, the GIF's 60 ms dropped" (decision 639).
    private static string EffectNoteOf(int frameCount, IReadOnlyList<GifFrame> stored)
    {
        var length = (frameCount * EffectTiming.FrameTime).TotalSeconds.ToString("0.##", CultureInfo.InvariantCulture);
        var pace = EffectTiming.FrameTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        return $"{frameCount} frames, {length} s; effects play at {pace} ms a frame, "
            + $"the GIF's {GifFrameTiming.FrameMilliseconds(stored)} ms dropped";
    }

    // The frames as scaled, each frame's width, and the frames as stored, whose delays give the time.
    private sealed record PreparedGif(IReadOnlyList<uint[]> Frames, int FrameWidth, IReadOnlyList<GifFrame> Stored);
}
