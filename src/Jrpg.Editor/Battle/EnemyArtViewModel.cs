using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Battle;

/// <summary>
/// An enemy's three pictures (its battle sprite, portrait and attack effect strip) by file name, whether the sprite is
/// drawn mirrored, and its battle sprite's idle loop as typed (ADR 0047). A name the folder lacks is allowed and marked
/// (decision 442): the game's start names it, as it refuses a strip that doesn't split into the loop's frames.
/// </summary>
internal sealed partial class EnemyArtViewModel : ObservableObject
{
    // What the boxes start at for an enemy without a loop (design tunable "New idle boxes"): the fewest frames a loop
    // may have, at the effect strips' pace.
    private const int NewFrameCount = TileAnimation.MinFrames;
    private const int NewFrameMilliseconds = 100;

    public EnemyArtViewModel(EnemyDefinition enemy)
    {
        BattleSpriteFile = enemy.BattleSpriteFile;
        PortraitFile = enemy.PortraitFile;
        AttackEffectFile = enemy.AttackEffectFile;
        IsSpriteFlipped = enemy.IsSpriteFlipped;
        IsAnimated = enemy.IdleAnimation is not null;
        FrameCountText = TypedNumber.Text(enemy.IdleAnimation?.FrameCount ?? NewFrameCount);
        FrameMillisecondsText = TypedNumber.Text(enemy.IdleAnimation?.FrameMilliseconds ?? NewFrameMilliseconds);
    }

    [ObservableProperty]
    public partial string BattleSpriteFile { get; set; }

    [ObservableProperty]
    public partial string PortraitFile { get; set; }

    [ObservableProperty]
    public partial string AttackEffectFile { get; set; }

    [ObservableProperty]
    public partial bool IsSpriteFlipped { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(PreviewLoop))]
    public partial bool IsAnimated { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(PreviewLoop))]
    public partial string FrameCountText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(PreviewLoop))]
    public partial string FrameMillisecondsText { get; set; }

    /// <summary>A loop box that isn't a whole number while animated; the ranges are Data's rules.</summary>
    public string? Problem =>
        IsAnimated
            ? TypedNumber.ProblemOf("Frames", FrameCountText) ?? TypedNumber.ProblemOf("Frame time", FrameMillisecondsText)
            : null;

    /// <summary>
    /// The loop the preview plays: the boxes as read, null while not animated or while a box can't give a frame (not a
    /// number, or under 1), so a redraw never throws on what is being typed.
    /// </summary>
    public TileAnimation? PreviewLoop => TypedLoop() is { FrameCount: > 0, FrameMilliseconds: > 0 } loop ? loop : null;

    /// <summary>Puts a file just imported into the box of the folder it went to.</summary>
    /// <param name="artFolder">The battlers, portraits or effects folder.</param>
    /// <param name="file">The imported file's name.</param>
    /// <exception cref="ArgumentException">Another folder: no enemy picture comes from it.</exception>
    public void Pick(string artFolder, string file)
    {
        switch (artFolder)
        {
            case ArtFolders.Battlers:
                BattleSpriteFile = file;
                break;
            case ArtFolders.Portraits:
                PortraitFile = file;
                break;
            case ArtFolders.Effects:
                AttackEffectFile = file;
                break;
            default:
                throw new ArgumentException($"No enemy picture comes from the '{artFolder}' folder.", nameof(artFolder));
        }
    }

    /// <summary>Fills the loop's boxes from a strip just imported from a GIF, which says its frames and time.</summary>
    public void TakeLoop(TileAnimation loop)
    {
        IsAnimated = true;
        FrameCountText = TypedNumber.Text(loop.FrameCount);
        FrameMillisecondsText = TypedNumber.Text(loop.FrameMilliseconds);
    }

    public void CopyFrom(EnemyArtViewModel chosen)
    {
        BattleSpriteFile = chosen.BattleSpriteFile;
        PortraitFile = chosen.PortraitFile;
        AttackEffectFile = chosen.AttackEffectFile;
        IsSpriteFlipped = chosen.IsSpriteFlipped;
        IsAnimated = chosen.IsAnimated;
        FrameCountText = chosen.FrameCountText;
        FrameMillisecondsText = chosen.FrameMillisecondsText;
    }

    /// <summary>
    /// The record with these pictures and loop over it, the names trimmed; no loop while not animated.
    /// </summary>
    public EnemyDefinition ApplyTo(EnemyDefinition enemy) => enemy with
    {
        BattleSpriteFile = BattleSpriteFile.Trim(),
        PortraitFile = PortraitFile.Trim(),
        AttackEffectFile = AttackEffectFile.Trim(),
        IsSpriteFlipped = IsSpriteFlipped,
        IdleAnimation = TypedLoop(),
    };

    // Every whole number as typed, so Data's check names a count or time out of range.
    private TileAnimation? TypedLoop() =>
        IsAnimated && TypedNumber.Parse(FrameCountText) is { } count && TypedNumber.Parse(FrameMillisecondsText) is { } time
            ? new TileAnimation { FrameCount = count, FrameMilliseconds = time }
            : null;
}
