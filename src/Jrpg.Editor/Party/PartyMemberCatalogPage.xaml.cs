using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Party;

/// <summary>
/// The Party members page (decision 953): the members left with Add and Remove, the chosen member's boxes right: id,
/// name, class by name, summary, and its three pictures by file over their folders, with a mark for each the folders
/// lack (decision 442), its battle sprite and portrait previewed under them. Its host shows the problem line and keeps
/// or drops the session. Each picture is read once per page, again once an import replaced it; each Import... beside a
/// box copies one PNG into its folder and picks it there, a file copy that needs no apply first.
/// </summary>
internal sealed partial class PartyMemberCatalogPage : UserControl
{
    /// <summary>
    /// The previews' width, the size each picture is decoded at. A double, as the XAML sets a <c>Width</c> from it and
    /// WPF refuses an int there, failing as the page loads.
    /// </summary>
    public const double PreviewWidth = 160;

    private readonly PartyMemberCatalogViewModel _members;
    private readonly Func<string, IReadOnlyList<string>, GifImportOptions, SheetImportResult> _import;
    private readonly DecodedArtImages _battlers;
    private readonly DecodedArtImages _portraits;

    /// <param name="members">The page's state, from <see cref="PartyMemberEditingViewModel.Start"/>.</param>
    /// <param name="import">
    /// The shell's import: copies the chosen images into an art folder, by folder name, and says what it did.
    /// </param>
    /// <param name="loadPicture">
    /// Decodes a picture of an art folder at <see cref="PreviewWidth"/>, by folder and file name; null when it can't.
    /// </param>
    public PartyMemberCatalogPage(
        PartyMemberCatalogViewModel members,
        Func<string, IReadOnlyList<string>, GifImportOptions, SheetImportResult> import,
        Func<string, string, BitmapSource?> loadPicture)
    {
        InitializeComponent();
        DataContext = _members = members;
        _import = import;
        _battlers = new DecodedArtImages(file => loadPicture(ArtFolders.Battlers, file));
        _portraits = new DecodedArtImages(file => loadPicture(ArtFolders.Portraits, file));
        members.PropertyChanged += OnMembersChanged;
        ShowPictures();
    }

    // The note is raised whenever a picture may change: another member chosen, a typed field, an import taken.
    private void OnMembersChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PartyMemberCatalogViewModel.MissingArtNote))
        {
            ShowPictures();
        }
    }

    private void ShowPictures()
    {
        var chosen = _members.SelectedMember;
        BattlerPreview.Source = PictureOf(_battlers, chosen?.BattleSpriteFile, _members.ArtChoices.Battlers);
        PortraitPreview.Source = PictureOf(_portraits, chosen?.PortraitFile, _members.ArtChoices.Portraits);
    }

    // A name the folder lacks is never read: Windows would open one named in another case (decision 895).
    private static BitmapSource? PictureOf(DecodedArtImages pictures, string? file, IReadOnlyList<string> folder) =>
        file?.Trim() is { } name && folder.Contains(name, StringComparer.Ordinal) ? pictures.Of(name) : null;

    // A replaced picture is forgotten before the import is taken, so the preview reads the new one.
    private void OnImportClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string artFolder }
            || ArtImageFileDialog.ChoosePng(Window.GetWindow(this), $"Import into the {artFolder} art") is not { } path)
        {
            return;
        }

        var result = _import(artFolder, [path], GifImportOptions.Default);
        ImportStatusText.Text = result.Summary;
        PicturesIn(artFolder)?.Forget(result.Imported);
        _members.TakeImport(artFolder, result);
    }

    // The attack effect has no preview, so nothing is held for its folder.
    private DecodedArtImages? PicturesIn(string artFolder) => artFolder switch
    {
        ArtFolders.Battlers => _battlers,
        ArtFolders.Portraits => _portraits,
        _ => null,
    };
}
