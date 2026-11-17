using System.IO;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Audio;
using Jrpg.Data.Content;
using Jrpg.Editor.Animation;
using Jrpg.Editor.Backgrounds;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Cinematics;
using Jrpg.Editor.Effects;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Items;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Skills;
using Jrpg.Editor.Statuses;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Database menu's pages over the draft (ADR 0045, decision 583): the shops, the drop tables, the enemies, the
/// cinematics (ADR 0046), the effect strips (decision 659), the skills, the tiered effects, the statuses (decision
/// 976), the items (decision 877), the affixes (decision 884) and the battle backgrounds (decision 901), beside the
/// quests; and the image import the Enemies, Effect strips, Character sprites and Battle backgrounds pages share, and
/// the strip Strip from sheet cells writes, through Data, asking before either replaces an image of the art folder.
/// </summary>
internal sealed partial class MainViewModel
{
    public ShopEditingViewModel? ShopEditing { get; }

    public EnemyEditingViewModel? EnemyEditing { get; }

    public DropTableEditingViewModel? DropTableEditing { get; }

    public CinematicEditingViewModel? CinematicEditing { get; }

    public EffectStripEditingViewModel? EffectStripEditing { get; }

    public SkillEditingViewModel? SkillEditing { get; }

    public TieredEffectEditingViewModel? TieredEffectEditing { get; }

    public StatusEditingViewModel? StatusEditing { get; }

    public ItemEditingViewModel? ItemEditing { get; }

    public AffixEditingViewModel? AffixEditing { get; }

    public BattleBackgroundEditingViewModel? BattleBackgroundEditing { get; }

    /// <summary>
    /// Copies images into an art folder (asking before replacing one, decisions 125 and 590); a file copy, not an undo
    /// step. The art warning is read again and the outcome goes to the status bar, as the tilesheet import's.
    /// </summary>
    /// <param name="artFolder">The field sprites, battlers, portraits, effects or backgrounds folder.</param>
    /// <param name="paths">The images chosen in the file dialog.</param>
    /// <param name="gifOptions">How a GIF chosen for the battlers or the effects becomes a strip (ADR 0047).</param>
    public SheetImportResult ImportArtImages(string artFolder, IReadOnlyList<string> paths, GifImportOptions gifOptions)
    {
        var result = new ArtImageImport(_writer, Content!)
            .Import(artFolder, paths, gifOptions, file => ConfirmsReplacing(artFolder, file));
        ReportArtImport(result);
        return result;
    }

    /// <summary>
    /// Writes the cells Strip from sheet cells cut as a strip into the effects folder (decision 990), asking before it
    /// replaces an image and reporting as <see cref="ImportArtImages"/>; a file copy, not an undo step. The result
    /// carries the strip's pace as its loop, which the Effect strips page takes as a typed row (decision 988).
    /// </summary>
    /// <param name="strip">The strip's name, cell size, cells and pace, as the tool's OK hands it.</param>
    /// <param name="frames">The cells' pixels in play order, as <see cref="SheetCells.Cut"/> gives them.</param>
    public SheetImportResult ImportCellStrip(CellStrip strip, IReadOnlyList<uint[]> frames)
    {
        var result = _writer.HasArtImage(ArtFolders.Effects, strip.FileName)
            && !ConfirmsReplacing(ArtFolders.Effects, strip.FileName)
                ? new SheetImportResult([], [])
                : WriteCellStrip(strip, frames);
        ReportArtImport(result);
        return result;
    }

    // A refusal is the result's, as an art image import's, so the tool shows it and stays open.
    private SheetImportResult WriteCellStrip(CellStrip strip, IReadOnlyList<uint[]> frames)
    {
        try
        {
            new CellStripImport(_writer, Content!).Write(frames, strip.CellSize, strip.FileName);
        }
        catch (Exception exception)
            when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
        {
            return new SheetImportResult([], [exception.Message]);
        }

        var loop = new TileAnimation { FrameCount = frames.Count, FrameMilliseconds = strip.FrameMilliseconds };
        return new SheetImportResult([strip.FileName], [])
        {
            Animations = new Dictionary<string, TileAnimation>(StringComparer.Ordinal) { [strip.FileName] = loop },
        };
    }

    // The question every art image import asks before it replaces an image of the folder (decisions 125 and 590).
    private bool ConfirmsReplacing(string artFolder, string file) =>
        _confirm($"{file} is already in the {artFolder} art. Replace it?");

    // The art warning read again once a file was copied, and the outcome on the status bar.
    private void ReportArtImport(SheetImportResult result)
    {
        if (result.Imported.Count > 0)
        {
            Art!.Refresh();
        }

        ImportStatus = result.Summary;
    }

    // Read when the page opens, so a picture or voice line dropped in since the editor started is offered; each voice's
    // length is read once per page, not on every key typed in the Voice box.
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

    // Read when the page opens, so an image or sound dropped in since the editor started is offered.
    private SkillChoices ReadSkillChoices()
    {
        var current = Content!.Current;
        var skills = current.Skills.Values;
        return new(
            SkillFileChoices.Of(ArtFolders.ImagesIn(ArtRoot, ArtFolders.Icons), skills.Select(skill => skill.IconFile)),
            SkillFileChoices.Of(AudioFolders.FilesIn(_audioRoot, AudioFolders.Effects), skills.Select(skill => skill.SoundFile)),
            SkillFileChoices.Of(ArtFolders.ImagesIn(ArtRoot, ArtFolders.Effects), skills.Select(skill => skill.EffectFile ?? "")),
            current.EffectStrips,
            ResourceChoice.AllIn(current),
            TierChoice.AllIn(current),
            StatusChoice.AllIn(current),
            current.Statuses);
    }

    private EffectStripEditingViewModel CreateEffectStripEditing() =>
        new(Content!, () => ReadableImagesIn(ArtFolders.Effects), file => ReadableSizeOf(ArtFolders.Effects, file));

    private BattleBackgroundEditingViewModel CreateBattleBackgroundEditing() =>
        new(
            Content!,
            () => ReadableImagesIn(ArtFolders.Backgrounds),
            file => ReadableSizeOf(ArtFolders.Backgrounds, file));

    private ItemEditingViewModel CreateItemEditing() =>
        new(Content!, () => ArtFolders.ImagesIn(ArtRoot, ArtFolders.Icons), () => ReadableImagesIn(ArtFolders.Effects));

    // An image the folder holds but can't be read is left out of an art page's list, as its size is unknown; both are
    // read when the page opens, never cached, since an import changes them (the Character sprites, Effect strips and
    // Battle backgrounds pages, and the Items page's effect strips).
    private IReadOnlyList<string> ReadableImagesIn(string artFolder) =>
        [.. ArtFolders.ImagesIn(ArtRoot, artFolder).Where(file => PngSize.IsReadable(ArtPath(artFolder, file)))];

    private (int Width, int Height)? ReadableSizeOf(string artFolder, string file) =>
        PngSize.IsReadable(ArtPath(artFolder, file)) ? PngSize.Read(ArtPath(artFolder, file)) : null;

    private string ArtPath(string artFolder, string file) => Path.Combine(ArtRoot, artFolder, file);
}
