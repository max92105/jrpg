using System.IO;
using Jrpg.Data.Art;
using Jrpg.Data.Audio;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Cinematics;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Database menu's windows over the draft (ADR 0045, decision 583): the shops, the drop tables, the enemies, the
/// cinematics (ADR 0046) and the effect strips (decision 659), beside the quests; and the image import the Enemies and
/// Character sprites windows share, through Data, asking first when it would replace an image.
/// </summary>
internal sealed partial class MainViewModel
{
    public ShopEditingViewModel? ShopEditing { get; }

    /// <summary>Whether Database > Shops... can open its window: false while the content didn't load.</summary>
    public bool HasShopEditing => ShopEditing is not null;

    public EnemyEditingViewModel? EnemyEditing { get; }

    /// <summary>Whether Database > Enemies... can open its window: false while the content didn't load.</summary>
    public bool HasEnemyEditing => EnemyEditing is not null;

    public DropTableEditingViewModel? DropTableEditing { get; }

    /// <summary>Whether Database > Drop tables... can open its window: false while the content didn't load.</summary>
    public bool HasDropTableEditing => DropTableEditing is not null;

    public CinematicEditingViewModel? CinematicEditing { get; }

    /// <summary>Whether Database > Cinematics... can open its window: false while the content didn't load.</summary>
    public bool HasCinematicEditing => CinematicEditing is not null;

    public EffectStripEditingViewModel? EffectStripEditing { get; }

    /// <summary>Whether Database > Effect strips... can open its window: false while the content didn't load.</summary>
    public bool HasEffectStripEditing => EffectStripEditing is not null;

    /// <summary>
    /// Copies images into an art folder (asking before replacing one, decisions 125 and 590); a file copy, not an undo
    /// step. The art warning is read again and the outcome goes to the status bar, as the tilesheet import's.
    /// </summary>
    /// <param name="artFolder">The field sprites, battlers, portraits or effects folder.</param>
    /// <param name="paths">The images chosen in the file dialog.</param>
    /// <param name="gifOptions">How a GIF chosen for the battlers becomes a strip (ADR 0047).</param>
    public SheetImportResult ImportArtImages(string artFolder, IReadOnlyList<string> paths, GifImportOptions gifOptions)
    {
        var result = new ArtImageImport(_writer, Content!)
            .Import(artFolder, paths, gifOptions, file => _confirm($"{file} is already in the {artFolder} art. Replace it?"));
        if (result.Imported.Count > 0)
        {
            Art!.Refresh();
        }

        ImportStatus = result.Summary;
        return result;
    }

    // Read when the window opens, so a picture or voice line dropped in since the editor started is offered; each
    // voice's length is read once per window, not on every key typed in the Voice box.
    private CinematicMediaChoices ReadCinematicMedia()
    {
        var lengths = new Dictionary<string, TimeSpan?>(StringComparer.Ordinal);
        return new(
            () => ArtFolders.ImagesUnder(ArtRoot, ArtFolders.Cinematics),
            () => AudioFolders.FilesIn(_audioRoot, AudioFolders.Voices),
            () => AudioFolders.TracksIn(_audioRoot),
            voice => lengths.TryGetValue(voice, out var length)
                ? length
                : lengths[voice] = WavDuration.Read(Path.Combine(_audioRoot, AudioFolders.Voices, voice)));
    }

    // An image the folder holds but can't be read is left out, as the Character sprites window's: its size is unknown.
    private EffectStripEditingViewModel CreateEffectStripEditing() =>
        new(
            Content!,
            () => [.. ArtFolders.ImagesIn(ArtRoot, ArtFolders.Effects).Where(file => PngSize.IsReadable(EffectPath(file)))],
            file => PngSize.IsReadable(EffectPath(file)) ? PngSize.Read(EffectPath(file)) : null);

    private string EffectPath(string file) => Path.Combine(ArtRoot, ArtFolders.Effects, file);
}
