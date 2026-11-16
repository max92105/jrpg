using System.IO;
using Jrpg.Data.Art;
using Jrpg.Data.Audio;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Cinematics;
using Jrpg.Editor.Effects;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Skills;
using Jrpg.Editor.Statuses;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Database menu's pages over the draft (ADR 0045, decision 583): the shops, the drop tables, the enemies, the
/// cinematics (ADR 0046), the effect strips (decision 659), the skills, the tiered effects and the statuses (decision
/// 976), beside the quests; and the image import the Enemies, Effect strips and Character sprites pages share, through
/// Data, asking first before it replaces any image of the art folder.
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

    /// <summary>
    /// Copies images into an art folder (asking before replacing one, decisions 125 and 590); a file copy, not an undo
    /// step. The art warning is read again and the outcome goes to the status bar, as the tilesheet import's.
    /// </summary>
    /// <param name="artFolder">The field sprites, battlers, portraits or effects folder.</param>
    /// <param name="paths">The images chosen in the file dialog.</param>
    /// <param name="gifOptions">How a GIF chosen for the battlers or the effects becomes a strip (ADR 0047).</param>
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

    // An image the folder holds but can't be read is left out, as the Character sprites page's: its size is unknown.
    private EffectStripEditingViewModel CreateEffectStripEditing() =>
        new(
            Content!,
            () => [.. ArtFolders.ImagesIn(ArtRoot, ArtFolders.Effects).Where(file => PngSize.IsReadable(EffectPath(file)))],
            file => PngSize.IsReadable(EffectPath(file)) ? PngSize.Read(EffectPath(file)) : null);

    private string EffectPath(string file) => Path.Combine(ArtRoot, ArtFolders.Effects, file);
}
