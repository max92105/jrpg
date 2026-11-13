using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Battle;

/// <summary>
/// An enemy's three pictures (its battle sprite, portrait and attack effect strip) by file name, and whether the sprite
/// is drawn mirrored. A name the folder lacks is allowed and marked (decision 442): the game's start names it.
/// </summary>
internal sealed partial class EnemyArtViewModel : ObservableObject
{
    public EnemyArtViewModel(EnemyDefinition enemy)
    {
        BattleSpriteFile = enemy.BattleSpriteFile;
        PortraitFile = enemy.PortraitFile;
        AttackEffectFile = enemy.AttackEffectFile;
        IsSpriteFlipped = enemy.IsSpriteFlipped;
    }

    [ObservableProperty]
    public partial string BattleSpriteFile { get; set; }

    [ObservableProperty]
    public partial string PortraitFile { get; set; }

    [ObservableProperty]
    public partial string AttackEffectFile { get; set; }

    [ObservableProperty]
    public partial bool IsSpriteFlipped { get; set; }

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

    public void CopyFrom(EnemyArtViewModel chosen)
    {
        BattleSpriteFile = chosen.BattleSpriteFile;
        PortraitFile = chosen.PortraitFile;
        AttackEffectFile = chosen.AttackEffectFile;
        IsSpriteFlipped = chosen.IsSpriteFlipped;
    }

    /// <summary>The record with these pictures over it, the names trimmed.</summary>
    public EnemyDefinition ApplyTo(EnemyDefinition enemy) => enemy with
    {
        BattleSpriteFile = BattleSpriteFile.Trim(),
        PortraitFile = PortraitFile.Trim(),
        AttackEffectFile = AttackEffectFile.Trim(),
        IsSpriteFlipped = IsSpriteFlipped,
    };
}
