using System.Globalization;
using System.IO;
using Jrpg.Core.Battle;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// One GIF into a strip (ADR 0047): read, made whole, its background cleared when asked, scaled to the chosen height,
/// and written as a PNG of the GIF's name into a fresh temporary folder. A battle sprite's strip carries its loop, at
/// the pace typed in the dialog or the GIF's most common delay; an attack effect's frames are each padded to a square
/// and play at the effects' own pace, the GIF's time dropped (decision 639), or carry the pace typed on the Effect
/// strips page as a loop (decision 988). WPF imaging needs an STA thread.
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
        var frameTime = options.FrameMilliseconds ?? GifFrameTiming.FrameMilliseconds(gif.Stored);
        var loop = new TileAnimation { FrameCount = gif.Frames.Count, FrameMilliseconds = frameTime };
        var size = $"{loop.FrameCount} frames of {gif.FrameWidth} x {options.BattleHeight} px";
        return new ConvertedGif(stripPath, loop, $"{size}, {PaceNoteOf(options.FrameMilliseconds, gif.Stored)}");
    }

    /// <exception cref="InvalidDataException">
    /// The GIF can't be read, or has one frame; the message names it.
    /// </exception>
    public static ConvertedGif ConvertToEffect(string gifPath, GifImportOptions options)
    {
        var gif = Prepared(gifPath, options);
        var stripPath = Written(
            gifPath, path => FrameStrip.WriteSquarePng(gif.Frames, gif.FrameWidth, options.BattleHeight, path));
        if (options.FrameMilliseconds is not { } typed)
        {
            return new ConvertedGif(stripPath, null, EffectNoteOf(gif.Frames.Count, gif.Stored));
        }

        var loop = new TileAnimation { FrameCount = gif.Frames.Count, FrameMilliseconds = typed };
        return new ConvertedGif(stripPath, loop, PacedEffectNoteOf(loop));
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

    // "the typed 110 ms" for a pace the developer set while watching the GIF play (decision 987), whatever its delays;
    // else "100 ms", and when the delays differed, that this is the most common (decision 632).
    private static string PaceNoteOf(int? typedFrameMilliseconds, IReadOnlyList<GifFrame> stored)
    {
        if (typedFrameMilliseconds is { } typed)
        {
            return $"the typed {typed} ms";
        }

        var frameTime = GifFrameTiming.FrameMilliseconds(stored);
        return GifFrameTiming.AreDelaysEqual(stored)
            ? $"{frameTime} ms"
            : $"{frameTime} ms; the GIF's frame times differed, {frameTime} ms is the most common";
    }

    // "12 frames, 0.96 s; effects play at 80 ms a frame unless effect-strips.json lists them, the GIF's 60 ms dropped"
    // (decision 639; ADR 0048 gives a listed strip its own pace).
    private static string EffectNoteOf(int frameCount, IReadOnlyList<GifFrame> stored)
    {
        var frameTime = EffectStripDefinition.UnlistedFrameTime;
        var length = (frameCount * frameTime).TotalSeconds.ToString("0.##", CultureInfo.InvariantCulture);
        var pace = frameTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        return $"{frameCount} frames, {length} s; effects play at {pace} ms a frame unless effect-strips.json lists "
            + $"them, the GIF's {GifFrameTiming.FrameMilliseconds(stored)} ms dropped";
    }

    // "3 frames, 0.33 s, paced at the typed 110 ms; the Effect strips page's OK writes its record": the pace typed on
    // that page becomes the strip's row there, a record only once the page applies (decision 988).
    private static string PacedEffectNoteOf(TileAnimation loop)
    {
        var length = (loop.FrameCount * loop.FrameDuration).TotalSeconds.ToString("0.##", CultureInfo.InvariantCulture);
        return $"{loop.FrameCount} frames, {length} s, paced at the typed {loop.FrameMilliseconds} ms; the Effect strips "
            + "page's OK writes its record";
    }

    // The frames as scaled, each frame's width, and the frames as stored, whose delays give the time.
    private sealed record PreparedGif(IReadOnlyList<uint[]> Frames, int FrameWidth, IReadOnlyList<GifFrame> Stored);
}
