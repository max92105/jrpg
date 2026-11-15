using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// The chosen still's picture for the Cinematics window's preview (ADR 0046): decoded once per file through the shell's
/// lookup while the window is open, null when the folder lacks it or it can't be read, so the window shows its note
/// instead (decision 442).
/// </summary>
/// <param name="load">Decodes a picture under the cinematics folder by its name there; null when it can't.</param>
internal sealed class CinematicPictureConverter(Func<string, BitmapSource?> load) : IValueConverter
{
    private readonly Dictionary<string, BitmapSource?> _decoded = new(StringComparer.Ordinal);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string { Length: > 0 } file)
        {
            return null;
        }

        if (!_decoded.TryGetValue(file, out var picture))
        {
            picture = load(file);
            _decoded[file] = picture;
        }

        return picture;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("The preview only shows a picture.");
}
