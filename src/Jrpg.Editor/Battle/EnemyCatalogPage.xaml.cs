using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Hosting;
using Microsoft.Win32;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Enemies page (ADR 0045): enemies on the left, the chosen one's Stats and Art tabs on the right. Its host shows
/// the problem line and keeps or drops the session. The art tab's marks and battle sprite preview follow the chosen
/// enemy, its pictures and the folders; an animated sprite's loop plays on the editor's animation clock, one frame of
/// the strip at a time, while the page is shown.
/// </summary>
internal sealed partial class EnemyCatalogPage : UserControl
{
    private readonly RenderTick _renderTick;
    private readonly EnemyCatalogViewModel _enemies;
    // The whole image shows while the sprite has no loop to play.
    private const int WholeImage = -1;

    private readonly EnemyArtSources _art;
    private readonly Action<Window, string> _editDropTables;
    private EnemyArtViewModel? _watchedArt;
    private BitmapSource? _battler;
    private TileAnimation? _loop;
    private int? _shownFrame;

    /// <param name="enemies">The page's state.</param>
    /// <param name="art">What the Art tab imports with, reads the battle sprite from and plays its loop on.</param>
    /// <param name="editDropTables">
    /// Opens the Drop tables page in a dialog over the given window, the page's own, on the given table id, and takes
    /// the draft's tables back when its OK applied (decision 602).
    /// </param>
    public EnemyCatalogPage(
        EnemyCatalogViewModel enemies,
        EnemyArtSources art,
        Action<Window, string> editDropTables)
    {
        InitializeComponent();
        _renderTick = new RenderTick(OnRendering);
        DataContext = _enemies = enemies;
        (_art, _editDropTables) = (art, editDropTables);
        enemies.PropertyChanged += OnEnemiesChanged;
        enemies.ArtChoices.PropertyChanged += (_, _) => ReadAndShowArt();
        WatchChosenArt(null);
        // WPF may raise Loaded twice, which Start ignores while it runs; a page shown again after Unloaded plays again.
        Loaded += (_, _) => _renderTick.Start();
        Unloaded += (_, _) => _renderTick.Stop();
    }

    // The dialog hosting the page owns what it opens (the mode bar plan's page recipe).
    private Window HostWindow => Window.GetWindow(this);

    // Another enemy chosen: its pictures are watched instead, so the battle sprite is read only when one changes.
    private void OnEnemiesChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EnemyCatalogViewModel.SelectedEnemy))
        {
            WatchChosenArt(_watchedArt);
        }
    }

    private void WatchChosenArt(EnemyArtViewModel? previous)
    {
        if (previous is not null)
        {
            previous.PropertyChanged -= OnChosenArtChanged;
        }

        _watchedArt = _enemies.SelectedEnemy?.Art;
        if (_watchedArt is not null)
        {
            _watchedArt.PropertyChanged += OnChosenArtChanged;
        }

        ReadAndShowArt();
    }

    // Only a new file name or folder reads the battle sprite again; a loop box or the flip redraws what is held.
    private void OnChosenArtChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EnemyArtViewModel.BattleSpriteFile))
        {
            ReadAndShowArt();
            return;
        }

        ShowArt();
    }

    private void ReadAndShowArt()
    {
        var art = _enemies.SelectedEnemy?.Art;
        _battler = art is null ? null : _art.BattlerImageOf(art.BattleSpriteFile.Trim());
        ShowArt();
    }

    // The marks for pictures the folders lack, and the battle sprite as the battle would mirror it.
    private void ShowArt()
    {
        var art = _enemies.SelectedEnemy?.Art;
        var missing = art is null ? [] : _enemies.ArtChoices.MissingOf(art);
        MissingText.Text = missing.Count == 0 ? "" : $"Not in the art folders: {string.Join(", ", missing)}.";
        _loop = art?.PreviewLoop;
        BattlerPreview.RenderTransform = new ScaleTransform(art is { IsSpriteFlipped: true } ? -1 : 1, 1);
        BattlerSizeText.Text = SizeLineOf(_battler, _loop);
        _shownFrame = null;
        ShowFrame();
    }

    // WPF raises Rendering once per frame it draws; the strip is cut again only when the loop moved to another frame.
    private void OnRendering(object? sender, EventArgs e) => ShowFrame();

    private void ShowFrame()
    {
        var frame = _loop is null || _battler is null ? WholeImage : _loop.FrameAt(_art.AnimationTime.Elapsed);
        if (frame == _shownFrame)
        {
            return;
        }

        _shownFrame = frame;
        BattlerPreview.Source = frame == WholeImage ? _battler : FrameOf(_battler!, _loop!, frame);
    }

    // A strip that doesn't split evenly still plays what it can: its frames are the whole-pixel width under the share.
    private static BitmapSource FrameOf(BitmapSource strip, TileAnimation loop, int frame)
    {
        var width = strip.PixelWidth / loop.FrameCount;
        return width == 0 ? strip : new CroppedBitmap(strip, new Int32Rect(frame * width, 0, width, strip.PixelHeight));
    }

    private static string SizeLineOf(BitmapSource? image, TileAnimation? loop) => (image, loop) switch
    {
        (null, _) => "",
        (_, null) => $"{image.PixelWidth} x {image.PixelHeight} px",
        _ when image.PixelWidth % loop.FrameCount == 0 =>
            $"{loop.FrameCount} frames of {image.PixelWidth / loop.FrameCount} x {image.PixelHeight} px",
        _ => $"{image.PixelWidth} px doesn't split into {loop.FrameCount} frames",
    };

    private void OnImportClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string artFolder })
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = $"Import into the {artFolder} art",
            Filter = ArtImageImport.TakesGifs(artFolder) ? "Images (*.png;*.gif)|*.png;*.gif" : "PNG images (*.png)|*.png",
            Multiselect = true,
        };
        if (dialog.ShowDialog(HostWindow) != true)
        {
            return;
        }

        if (AskGifOptions(artFolder, dialog.FileNames) is not { } options)
        {
            return;
        }

        var result = _art.Import(artFolder, dialog.FileNames, options);
        ImportStatusText.Text = result.Summary;
        _enemies.TakeImport(artFolder, result);
    }

    // A GIF asks its height and background first (decisions 637, 638); null when cancelled.
    private GifImportOptions? AskGifOptions(string artFolder, string[] paths)
    {
        var gif = paths.FirstOrDefault(path => Path.GetExtension(path).Equals(".gif", StringComparison.OrdinalIgnoreCase));
        if (!ArtImageImport.TakesGifs(artFolder) || gif is null)
        {
            return GifImportOptions.Default;
        }

        var options = new GifImportOptionsViewModel(HeightOf(gif));
        return GifImportWindow.Ask(HostWindow, options) ? options.ToOptions() : null;
    }

    // For the dialog's note only: a GIF that can't be read or opened leaves the note empty; the import then names it
    // among its refusals.
    private static int? HeightOf(string gifPath)
    {
        try
        {
            return GifReader.Read(gifPath).Height;
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    // The button sits in the chosen enemy's panel, greyed while none is chosen.
    private void OnEditDropTablesClicked(object sender, RoutedEventArgs e) =>
        _editDropTables(HostWindow, _enemies.SelectedEnemy!.DropTableId);
}
