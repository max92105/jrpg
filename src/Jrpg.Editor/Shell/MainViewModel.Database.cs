using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Database menu's windows over the draft (ADR 0045, decision 583): the shops and the enemies, beside the quests;
/// and the image import the Enemies and Character sprites windows share, through Data, asking before a replacement.
/// </summary>
internal sealed partial class MainViewModel
{
    public ShopEditingViewModel? ShopEditing { get; }

    /// <summary>Whether Database > Shops... can open its window: false while the content didn't load.</summary>
    public bool HasShopEditing => ShopEditing is not null;

    public EnemyEditingViewModel? EnemyEditing { get; }

    /// <summary>Whether Database > Enemies... can open its window: false while the content didn't load.</summary>
    public bool HasEnemyEditing => EnemyEditing is not null;

    /// <summary>
    /// Copies images into an art folder (asking before replacing one, decisions 125 and 590); a file copy, not an undo
    /// step. The art warning is read again and the outcome goes to the status bar, as the tilesheet import's.
    /// </summary>
    /// <param name="artFolder">The field sprites, battlers, portraits or effects folder.</param>
    /// <param name="paths">The images chosen in the file dialog.</param>
    public SheetImportResult ImportArtImages(string artFolder, IReadOnlyList<string> paths)
    {
        var result = new ArtImageImport(_writer, Content!)
            .Import(artFolder, paths, file => _confirm($"{file} is already in the {artFolder} art. Replace it?"));
        if (result.Imported.Count > 0)
        {
            Art!.Refresh();
        }

        ImportStatus = result.Summary;
        return result;
    }
}
