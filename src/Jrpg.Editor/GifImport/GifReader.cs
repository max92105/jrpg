using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// Reads a GIF's canvas size and frames through WPF's own decoder and its metadata (ADR 0047: no package), each frame
/// converted to BGRA, its transparent color with alpha 0. A metadata query the file lacks reads as absent, never a
/// throw: a canvas without its size takes the first frame's, a frame without a timing block has no delay and keeps the
/// canvas. WPF imaging needs an STA thread.
/// </summary>
internal static class GifReader
{
    private const int BytesPerPixel = 4;

    /// <exception cref="InvalidDataException">WPF can't decode the file as a GIF; the message names it.</exception>
    public static GifImage Read(string path)
    {
        using var stream = File.OpenRead(path);
        try
        {
            return Decoded(stream);
        }
        catch (Exception exception) when (exception is FileFormatException or NotSupportedException or COMException)
        {
            var problem = $"{Path.GetFileName(path)} isn't a GIF the editor can read: {exception.Message}";
            throw new InvalidDataException(problem, exception);
        }
    }

    // A frame can fail as it is converted, not only as the file opens, so the whole read is one refusal.
    private static GifImage Decoded(Stream stream)
    {
        var decoder = new GifBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        if (decoder.Frames.Count == 0)
        {
            throw new FileFormatException("No frame.");
        }

        var frames = decoder.Frames.Select(ReadFrame).ToList();
        var canvas = decoder.Metadata as BitmapMetadata;
        var first = decoder.Frames[0];
        var width = NumberAt(canvas, "/logscrdesc/Width") ?? first.PixelWidth;
        var height = NumberAt(canvas, "/logscrdesc/Height") ?? first.PixelHeight;
        return new GifImage(width, height, frames);
    }

    private static GifFrame ReadFrame(BitmapFrame frame)
    {
        var metadata = frame.Metadata as BitmapMetadata;
        var bgra = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
        var pixels = new uint[bgra.PixelWidth * bgra.PixelHeight];
        bgra.CopyPixels(pixels, bgra.PixelWidth * BytesPerPixel, 0);
        return new GifFrame(
            NumberAt(metadata, "/imgdesc/Left") ?? 0,
            NumberAt(metadata, "/imgdesc/Top") ?? 0,
            bgra.PixelWidth,
            bgra.PixelHeight,
            pixels,
            NumberAt(metadata, "/grctlext/Delay") ?? 0,
            DisposalOf(NumberAt(metadata, "/grctlext/Disposal")));
    }

    private static GifDisposal DisposalOf(int? stored) => stored switch
    {
        2 => GifDisposal.ClearToBackground,
        3 => GifDisposal.RestorePrevious,
        _ => GifDisposal.Keep,
    };

    // The GIF metadata holds its numbers as ushort or byte; anything else, or nothing, is absent.
    private static int? NumberAt(BitmapMetadata? metadata, string query)
    {
        if (metadata is null || !metadata.ContainsQuery(query))
        {
            return null;
        }

        return metadata.GetQuery(query) switch
        {
            ushort number => number,
            byte number => number,
            _ => null,
        };
    }
}
