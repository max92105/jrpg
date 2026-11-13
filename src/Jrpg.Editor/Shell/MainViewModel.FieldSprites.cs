using System.IO;
using Jrpg.Data.Art;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Objects > Character sprites...: the window's start and apply over the draft, and its Import... through Data, asking
/// before replacing an image, the outcome in the status bar as the tilesheet import's.
/// </summary>
internal sealed partial class MainViewModel
{
    public FieldSpriteEditingViewModel? FieldSpriteEditing { get; }

    public bool CanEditFieldSprites => FieldSpriteEditing is not null;

    /// <summary>
    /// Copies images into the field-sprite art (asking before replacing one); a file copy, not an undo step.
    /// </summary>
    /// <param name="paths">The images chosen in the file dialog.</param>
    public SheetImportResult ImportFieldSprites(IReadOnlyList<string> paths)
    {
        var result = new FieldSpriteImport(_writer, Content!)
            .Import(paths, file => _confirm($"{file} is already in the field-sprite art. Replace it?"));
        if (result.Imported.Count > 0)
        {
            Art!.Refresh();
            ImagesImported?.Invoke(this, result.Imported);
        }

        ImportStatus = result.Summary;
        return result;
    }

    // The folder's readable images and their sizes, read when the window opens, never cached: an import changes them.
    private FieldSpriteEditingViewModel CreateFieldSpriteEditing() =>
        new(
            Content!,
            () => [.. ArtFolders.FieldSpriteImagesIn(ArtRoot).Where(file => PngSize.IsReadable(FieldSpritePath(file)))],
            file => PngSize.IsReadable(FieldSpritePath(file)) ? PngSize.Read(FieldSpritePath(file)) : null);

    private string FieldSpritePath(string file) => Path.Combine(ArtRoot, ArtFolders.FieldSprites, file);
}
