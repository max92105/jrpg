using System.Globalization;
using System.IO;
using Jrpg.Core.Battle;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// One GIF into a strip (ADR 0047) or a sheet: read, made whole, its background cleared when asked, scaled to the
/// chosen height, and written as a PNG of the GIF's name into a fresh temporary folder. A battle sprite's strip carries
/// its loop, at the pace typed in the dialog or the GIF's most common delay; an attack effect's frames are each padded
/// to a square and play at the effects' own pace, the GIF's time dropped (decision 639), or carry the pace typed on the
/// Effect strips page as a loop (decision 988). A battle background's frames, read once for the dialog, are kept at
/// their height or averaged down, never cleared, and laid in a grid sheet with the record that describes it (ADR 0059).
/// WPF imaging needs an STA thread.
/// </summary>
internal static class GifConversion
{
    /// <summary>
    /// What a strip's or a sheet's temporary folder starts with, under the system's temporary folder.
    /// </summary>
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

    /// <param name="gifPath">The GIF read, whose name the sheet takes.</param>
    /// <param name="gif">Its frames as read once for the dialog (decision 987).</param>
    /// <param name="options">
    /// The frame height in the sheet, from 1 to the GIF's own, and the typed pace; the background box is ignored, since
    /// a background is opaque (decision 904).
    /// </param>
    /// <exception cref="InvalidDataException">The GIF has one frame; the message names it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A height above the GIF's: a background is never enlarged.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The frames don't fit one sheet (<see cref="FrameGrid.MostFrames"/>).
    /// </exception>
    public static ConvertedGif ConvertToBackground(string gifPath, ComposedGif gif, GifImportOptions options)
    {
        RefuseOneFrame(gifPath, gif.Frames.Count);
        var height = options.BattleHeight;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, gif.Height, nameof(options));
        var frames = height == gif.Height ? gif.Frames : FrameScaler.ScaleAveraged(gif.Frames, gif.Width, gif.Height, height);
        var width = FrameScaler.WidthAt(gif.Width, gif.Height, height);
        var sheetPath = Written(gifPath, path => FrameGrid.WritePng(frames, width, height, path));
        var frameTime = options.FrameMilliseconds ?? gif.FrameMilliseconds;
        var background = new AnimatedBackgroundDefinition
        {
            File = Path.GetFileName(sheetPath),
            FrameWidth = width,
            FrameHeight = height,
            Animation = new TileAnimation { FrameCount = frames.Count, FrameMilliseconds = frameTime },
        };
        var pace = options.FrameMilliseconds is null ? $"{frameTime} ms" : $"the typed {frameTime} ms";
        var note = $"{frames.Count} frames of {width} x {height} px, {pace}";
        return new ConvertedGif(sheetPath, null, note) { Background = background };
    }

    private static PreparedGif Prepared(string gifPath, GifImportOptions options)
    {
        var image = GifReader.Read(gifPath);
        RefuseOneFrame(gifPath, image.Frames.Count);
        var frames = GifFrameComposer.Compose(image);
        var cleared = options.RemovesBackground ? BackgroundRemoval.Apply(frames) : frames;
        var scaled = FrameScaler.Scale(cleared, image.Width, image.Height, options.BattleHeight);
        return new PreparedGif(scaled, FrameScaler.WidthAt(image.Width, image.Height, options.BattleHeight), image.Frames);
    }

    private static void RefuseOneFrame(string gifPath, int frameCount)
    {
        if (frameCount < TileAnimation.MinFrames)
        {
            throw new InvalidDataException($"{Path.GetFileName(gifPath)} has one frame: import a PNG.");
        }
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
