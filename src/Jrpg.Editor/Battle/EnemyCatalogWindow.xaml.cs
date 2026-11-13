using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Tiles;
using Microsoft.Win32;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Enemies window (ADR 0045): enemies on the left, the chosen one's Stats and Art tabs on the right. OK applies the
/// session as one undo step, greyed while a problem shows; Cancel drops it. The art tab's marks and battle sprite
/// preview follow the chosen enemy, its pictures and the folders.
/// </summary>
internal sealed partial class EnemyCatalogWindow : Window
{
    private readonly EnemyCatalogViewModel _enemies;
    private readonly Func<string, IReadOnlyList<string>, SheetImportResult> _import;
    private readonly Func<string, BitmapSource?> _battlerImageOf;
    private EnemyArtViewModel? _watchedArt;

    /// <param name="enemies">The window's state.</param>
    /// <param name="import">
    /// Copies chosen images into an art folder, by folder name, and says what it did (the shell's import).
    /// </param>
    /// <param name="battlerImageOf">A battle sprite read whole by file name; null when the folder lacks it.</param>
    public EnemyCatalogWindow(
        EnemyCatalogViewModel enemies,
        Func<string, IReadOnlyList<string>, SheetImportResult> import,
        Func<string, BitmapSource?> battlerImageOf)
    {
        InitializeComponent();
        DataContext = _enemies = enemies;
        (_import, _battlerImageOf) = (import, battlerImageOf);
        enemies.PropertyChanged += OnEnemiesChanged;
        enemies.ArtChoices.PropertyChanged += (_, _) => ShowArt();
        WatchChosenArt(null);
    }

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

        ShowArt();
    }

    private void OnChosenArtChanged(object? sender, PropertyChangedEventArgs e) => ShowArt();

    // The marks for pictures the folders lack, and the battle sprite as the battle would mirror it.
    private void ShowArt()
    {
        var art = _enemies.SelectedEnemy?.Art;
        var missing = art is null ? [] : _enemies.ArtChoices.MissingOf(art);
        MissingText.Text = missing.Count == 0 ? "" : $"Not in the art folders: {string.Join(", ", missing)}.";
        var image = art is null ? null : _battlerImageOf(art.BattleSpriteFile.Trim());
        BattlerPreview.Source = image;
        BattlerPreview.RenderTransform = new ScaleTransform(art is { IsSpriteFlipped: true } ? -1 : 1, 1);
        BattlerSizeText.Text = image is null ? "" : $"{image.PixelWidth} x {image.PixelHeight} px";
    }

    private void OnImportClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string artFolder })
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = $"Import into the {artFolder} art",
            Filter = "PNG images (*.png)|*.png",
            Multiselect = true,
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var result = _import(artFolder, dialog.FileNames);
        ImportStatusText.Text = result.Summary;
        _enemies.TakeImport(artFolder, result);
    }

    // OK is greyed while a problem shows, so a click always applies.
    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
