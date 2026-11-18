using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The files the three art boxes of the Enemies and Party members pages offer (decision 589's picker pattern): the
/// battlers, portraits and effects folders as they are, read through a lookup the shell gives, and read again once an
/// import copied a file in.
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
    /// The mark under an enemy's or a party member's art boxes (decision 442): "Not in the art folders: battle sprite
    /// Wolf.png, portrait (none).", each picture its folder lacks; empty when every one is there. Takes the names, so
    /// the rule holds for both without this type knowing a member.
    /// </summary>
    public string MissingNoteOf(string battleSpriteFile, string portraitFile, string attackEffectFile)
    {
        var missing = new List<string>();
        AddMissing(missing, "battle sprite", battleSpriteFile, Battlers);
        AddMissing(missing, "portrait", portraitFile, Portraits);
        AddMissing(missing, "attack effect", attackEffectFile, Effects);
        return missing.Count == 0 ? "" : $"Not in the art folders: {string.Join(", ", missing)}.";
    }

    // A new enemy or member names no picture yet: its mark says so rather than ending in a blank.
    private static void AddMissing(List<string> missing, string kind, string file, IReadOnlyList<string> folder)
    {
        var name = file.Trim();
        if (!folder.Contains(name, StringComparer.Ordinal))
        {
            missing.Add($"{kind} {(name.Length > 0 ? name : "(none)")}");
        }
    }
}
