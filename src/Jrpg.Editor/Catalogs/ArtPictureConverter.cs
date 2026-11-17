using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// An art file's name to its picture for a database page (the Cinematics page's still, ADR 0046; the Items page's icon,
/// decision 879): decoded once per file through the shell's lookup while the page is shown, null when the folder lacks
/// it or it can't be read, so the page shows its note in its place (decision 442).
/// </summary>
/// <param name="load">Decodes a picture of the page's art folder by its name there; null when it can't.</param>
internal sealed class ArtPictureConverter(Func<string, BitmapSource?> load) : IValueConverter
{
    private readonly DecodedArtImages _pictures = new(load);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string { Length: > 0 } file ? _pictures.Of(file) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("The page only shows a picture.");
}
