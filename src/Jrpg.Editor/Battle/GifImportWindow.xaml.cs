using System.ComponentModel;
using System.IO;
using System.Windows;
using Jrpg.Editor.Animation;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The GIF import's dialog, opened by Import... before a GIF becomes a strip: its height, background and pace boxes
/// beside the first chosen GIF playing in the shared preview (decision 987). OK is greyed while a box can't be read;
/// Cancel imports nothing.
/// </summary>
internal sealed partial class GifImportWindow : Window
{
    private readonly GifImportOptionsViewModel _options;

    private GifImportWindow(GifImportOptionsViewModel options)
    {
        InitializeComponent();
        DataContext = _options = options;
        options.PropertyChanged += OnOptionsChanged;
        ShowGif();
        Loaded += (_, _) =>
        {
            HeightBox.Focus();
            HeightBox.SelectAll();
        };
    }

    /// <summary>
    /// Asks how the chosen GIFs are converted, over its owner; no dialog, and the default options, when none is a GIF.
    /// </summary>
    /// <param name="owner">The window the dialog is centered on.</param>
    /// <param name="paths">The files chosen for the import, in order.</param>
    /// <param name="optionsFor">
    /// The boxes for the folder imported into, from the first GIF (null when it can't be read) and the GIFs' count.
    /// </param>
    /// <returns>The options OK applied; null on Cancel, so nothing is imported.</returns>
    public static GifImportOptions? AskFor(
        Window owner, IReadOnlyList<string> paths, Func<ComposedGif?, int, GifImportOptionsViewModel> optionsFor)
    {
        var gifs = paths.Where(path => Path.GetExtension(path).Equals(".gif", StringComparison.OrdinalIgnoreCase)).ToList();
        if (gifs.Count == 0)
        {
            return GifImportOptions.Default;
        }

        var options = optionsFor(ReadableGif(gifs[0]), gifs.Count);
        return new GifImportWindow(options) { Owner = owner }.ShowDialog() == true ? options.ToOptions() : null;
    }

    // For the preview and the notes only: a GIF that can't be opened or decoded shows none, and the import then names
    // it among its refusals.
    private static ComposedGif? ReadableGif(string path)
    {
        try
        {
            return ComposedGif.Read(path);
        }
        catch (Exception exception)
            when (exception is InvalidDataException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return null;
        }
    }

    private void OnOptionsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GifImportOptionsViewModel.RemovesBackground))
        {
            ShowGif();
        }
    }

    // Cleared as the import clears it, at its own size. New frames start the preview over, so the background box
    // restarts it at frame 0.
    private void ShowGif() =>
        GifPreview.Frames = _options.Gif is { } gif
            ? PreviewFrames.OfPixels(
                _options.RemovesBackground ? BackgroundRemoval.Apply(gif.Frames) : gif.Frames, gif.Width, gif.Height)
            : null;

    // OK is greyed while a problem shows, so a click always applies.
    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
