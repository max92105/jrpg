using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The files the Enemies page's three art boxes offer (decision 589's picker pattern): the battlers, portraits and
/// effects folders as they are, read through a lookup the shell gives, and read again after an import.
/// </summary>
internal sealed partial class BattleArtChoices : ObservableObject
{
    private readonly Func<string, IReadOnlyList<string>> _imagesIn;

    /// <param name="imagesIn">The PNG names of an art folder, by the folder's name (<see cref="ArtFolders"/>).</param>
    public BattleArtChoices(Func<string, IReadOnlyList<string>> imagesIn)
    {
        _imagesIn = imagesIn;
        Battlers = imagesIn(ArtFolders.Battlers);
        Portraits = imagesIn(ArtFolders.Portraits);
        Effects = imagesIn(ArtFolders.Effects);
    }

    [ObservableProperty]
    public partial IReadOnlyList<string> Battlers { get; private set; }

    [ObservableProperty]
    public partial IReadOnlyList<string> Portraits { get; private set; }

    [ObservableProperty]
    public partial IReadOnlyList<string> Effects { get; private set; }

    /// <summary>Reads the three folders again, after an import copied a file in.</summary>
    public void Refresh()
    {
        Battlers = _imagesIn(ArtFolders.Battlers);
        Portraits = _imagesIn(ArtFolders.Portraits);
        Effects = _imagesIn(ArtFolders.Effects);
    }

    /// <summary>
    /// Which of an enemy's pictures its folder lacks, as "battle sprite Wolf.png" lines for the marks (decision 442);
    /// empty when every one is there.
    /// </summary>
    public IReadOnlyList<string> MissingOf(EnemyArtViewModel art)
    {
        var missing = new List<string>();
        AddMissing(missing, "battle sprite", art.BattleSpriteFile, Battlers);
        AddMissing(missing, "portrait", art.PortraitFile, Portraits);
        AddMissing(missing, "attack effect", art.AttackEffectFile, Effects);
        return missing;
    }

    // A new enemy names no picture yet: its mark says so rather than ending in a blank.
    private static void AddMissing(List<string> missing, string kind, string file, IReadOnlyList<string> folder)
    {
        var name = file.Trim();
        if (!folder.Contains(name, StringComparer.Ordinal))
        {
            missing.Add($"{kind} {(name.Length > 0 ? name : "(none)")}");
        }
    }
}
