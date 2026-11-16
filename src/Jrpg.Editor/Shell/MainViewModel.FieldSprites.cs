using System.IO;
using Jrpg.Data.Art;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Objects;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Shell;

/// <summary>
/// NPCs > Character sprites...: the page's start and apply over the draft, and its Import... through Data, asking
/// before replacing an image, the outcome in the status bar as the tilesheet import's.
/// </summary>
internal sealed partial class MainViewModel
{
    public FieldSpriteEditingViewModel? FieldSpriteEditing { get; }

    public bool CanEditFieldSprites => FieldSpriteEditing is not null;

    /// <summary>
    /// NPCs > Change NPC sprite... on the Objects panel's chosen NPC; null when the content didn't load.
    /// </summary>
    public NpcSpriteChange? NpcSpriteChange { get; }


    /// <summary>
    /// Copies images into the field-sprite art (asking before replacing one); a file copy, not an undo step.
    /// </summary>
    /// <param name="paths">The images chosen in the file dialog.</param>
    public SheetImportResult ImportFieldSprites(IReadOnlyList<string> paths)
    {
        var result = ImportArtImages(ArtFolders.FieldSprites, paths, GifImportOptions.Default);
        if (result.Imported.Count > 0)
        {
            ImagesImported?.Invoke(this, result.Imported);
        }

        return result;
    }

    // The folder's readable images and their sizes, read when the page opens, never cached: an import changes them.
    private FieldSpriteEditingViewModel CreateFieldSpriteEditing() =>
        new(
            Content!,
            () => [.. ArtFolders.FieldSpriteImagesIn(ArtRoot).Where(file => PngSize.IsReadable(FieldSpritePath(file)))],
            file => PngSize.IsReadable(FieldSpritePath(file)) ? PngSize.Read(FieldSpritePath(file)) : null);

    private string FieldSpritePath(string file) => Path.Combine(ArtRoot, ArtFolders.FieldSprites, file);
}
