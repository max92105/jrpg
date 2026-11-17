using System.ComponentModel;
using System.IO;
using System.Windows;
using Jrpg.Editor.Animation;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The GIF import's dialog, opened by Import... before a GIF becomes a strip or a background's sheet: its height,
/// background and pace boxes beside the first chosen GIF playing in the shared preview (decision 987). A background's
/// kind chooses its frame height among decision 898's instead, hides the background box and names the sheet it makes
/// (decision 903). OK is greyed while a box can't be read or the sheet can't hold every frame; Cancel imports nothing.
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
        Loaded += (_, _) => FocusHeight();
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
        if (e.PropertyName is nameof(GifImportOptionsViewModel.RemovesBackground)
            or nameof(GifImportOptionsViewModel.PreviewAtChosenHeight))
        {
            ShowGif();
        }
    }

    // Cleared as the import clears it, at its own size; a background at the chosen height, as its sheet holds it, its
    // box hidden and never ticked. New frames start the preview over, so the background box or a new height restarts it
    // at frame 0.
    private void ShowGif() =>
        GifPreview.Frames = (_options.IsBackground ? _options.PreviewAtChosenHeight : _options.Gif) is { } gif
            ? PreviewFrames.OfPixels(
                _options.RemovesBackground ? BackgroundRemoval.Apply(gif.Frames) : gif.Frames, gif.Width, gif.Height)
            : null;

    private void FocusHeight()
    {
        if (_options.IsBackground)
        {
            HeightChoiceBox.Focus();
            return;
        }

        HeightBox.Focus();
        HeightBox.SelectAll();
    }

    // OK is greyed while a problem shows, so a click always applies.
    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
