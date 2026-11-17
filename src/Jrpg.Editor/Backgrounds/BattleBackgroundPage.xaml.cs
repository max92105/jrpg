using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;
using Jrpg.Editor.Animation;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Backgrounds;

/// <summary>
/// The Battle backgrounds page (decisions 901, 902): the backgrounds folder's images on the left, the chosen one's
/// kind, frame time and preview on the right. Its host shows the problem line and keeps or drops the session. The
/// chosen one plays in the shared preview (<see cref="FramePreview"/>, decision 986) at its typed frame time, cover-fit
/// in a 16:9 area as the battle draws it; a still shows as its one frame. An image is read once per page, again once an
/// import replaced it. Import... under the list copies PNGs into the folder as stills, or one GIF as an animated
/// background's sheet once the GIF dialog chose its frame height and pace (decision 903), and hands them to the list.
/// </summary>
internal sealed partial class BattleBackgroundPage : UserControl
{
    /// <summary>The preview area's width, a quarter of the 1920 px screen the battle covers.</summary>
    public const double PreviewWidth = 480;

    /// <summary>The preview area's height, the screen's 16:9 at <see cref="PreviewWidth"/>.</summary>
    public const double PreviewHeight = 270;

    private readonly BattleBackgroundCatalogViewModel _backgrounds;
    private readonly DecodedArtImages _images;
    private readonly Func<IReadOnlyList<string>, GifImportOptions, SheetImportResult> _import;

    /// <param name="backgrounds">The page's state, from <see cref="BattleBackgroundEditingViewModel.Start"/>.</param>
    /// <param name="loadImage">Decodes a backgrounds image at its own size; null when it can't.</param>
    /// <param name="import">
    /// The shell's import: copies the chosen PNGs into the backgrounds folder, a GIF as a sheet by the options, and
    /// says what it did.
    /// </param>
    public BattleBackgroundPage(
        BattleBackgroundCatalogViewModel backgrounds,
        Func<string, BitmapSource?> loadImage,
        Func<IReadOnlyList<string>, GifImportOptions, SheetImportResult> import)
    {
        InitializeComponent();
        DataContext = _backgrounds = backgrounds;
        (_images, _import) = (new DecodedArtImages(loadImage), import);
        backgrounds.PropertyChanged += OnBackgroundsChanged;
        ShowChosenBackground();
    }

    private void OnBackgroundsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BattleBackgroundCatalogViewModel.SelectedBackground))
        {
            ShowChosenBackground();
        }
    }

    // Only a new choice hands the preview new frames: a typed frame time reaches it through its binding and keeps the
    // shown frame. A row's kind changes only with the row (an import builds a new one), so the choice covers it too. A
    // missing image is never read: Windows would open one named in another case (decision 895).
    private void ShowChosenBackground() =>
        BackgroundPreview.Frames =
            _backgrounds.SelectedBackground is { IsImageMissing: false } background && _images.Of(background.File) is { } image
                ? FramesOf(background, image)
                : null;

    // An animated one whose grid has a problem has no cells, so it shows nothing while the problem line says why.
    private static IReadOnlyList<BitmapSource> FramesOf(BattleBackgroundViewModel background, BitmapSource image) =>
        background.FrameSize is { } frameSize ? PreviewFrames.OfCells(image, frameSize, background.PreviewCells) : [image];

    // A file copy is not a draft change, so the page's session needs no apply first; a replaced image is read again
    // before its row is chosen, so the preview shows the new picture.
    private void OnImportClicked(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        if (ArtImageFileDialog.Choose(owner, "Import battle backgrounds", ArtFolders.Backgrounds) is not { } paths
            || GifImportWindow.AskFor(owner, paths, (first, _) => GifImportOptionsViewModel.ForBackground(first))
                is not { } options)
        {
            return;
        }

        var result = _import(paths, options);
        ImportStatusText.Text = result.Summary;
        _images.Forget(result.Imported);

        _backgrounds.TakeImport(result);
    }
}
