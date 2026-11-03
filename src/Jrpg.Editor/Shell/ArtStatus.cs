using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The art problems as the status band's warning, the same lines the game would stop on, and what is missing for the
/// drawing (decision 442: missing art is the editor's warning, the game's error). Refreshed when the sheets or objects
/// change, and on request after an import or a save.
/// </summary>
internal sealed partial class ArtStatus : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly string _artRoot;
    private IReadOnlyDictionary<string, TilesheetDefinition> _checkedSheets;
    private IReadOnlyDictionary<string, ObjectDefinition> _checkedObjects;

    /// <param name="catalog">The content whose art is checked.</param>
    /// <param name="artRoot">The <c>art</c> folder.</param>
    public ArtStatus(CatalogDraft catalog, string artRoot)
    {
        _catalog = catalog;
        _artRoot = artRoot;
        Refresh();
        catalog.PropertyChanged += OnCatalogChanged;
    }

    /// <summary>One line per missing or unusable file; null while the art is whole.</summary>
    [ObservableProperty]
    public partial string? Warning { get; private set; }

    [ObservableProperty]
    public partial MissingArt Missing { get; private set; }

    /// <summary>Checks the art folder again, for the caller that changed it (an import, a save).</summary>
    [MemberNotNull(nameof(_checkedSheets), nameof(_checkedObjects), nameof(Missing))]
    public void Refresh()
    {
        var catalog = _catalog.Current;
        _checkedSheets = catalog.Tilesheets;
        _checkedObjects = catalog.Objects;
        var problems = ArtReferenceCheck.FindProblems(catalog, _artRoot);
        Warning = problems.Count == 0 ? null : string.Join(Environment.NewLine, problems);
        Missing = MissingArt.Of(
            catalog,
            file => PngSize.IsReadable(Path.Combine(_artRoot, ArtFolders.Tilesheets, file)),
            file => PngSize.IsReadable(Path.Combine(_artRoot, ArtFolders.FieldSprites, file)));
    }

    /// <summary>
    /// The cursor status's note for a tile the drawing marks: three spaces, then "(sheet city: image missing)"; empty
    /// for any other.
    /// </summary>
    public string NoteFor(int tileId)
    {
        var sheetId = _catalog.Current.Tiles[tileId].SheetId;
        return Missing.IsSheetMissing(sheetId) ? $"   (sheet {sheetId}: image missing)" : "";
    }

    // Only the sheets and objects name art; a painted tile replaces the catalog too, and needs no file read.
    private void OnCatalogChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogDraft.Current)
            && (!ReferenceEquals(_catalog.Current.Tilesheets, _checkedSheets) || !ReferenceEquals(_catalog.Current.Objects, _checkedObjects)))
        {
            Refresh();
        }
    }
}
