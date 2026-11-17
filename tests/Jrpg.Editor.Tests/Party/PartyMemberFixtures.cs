using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Tests.Party;

/// <summary>
/// The Party members page tests' own member, so no repository member is pinned: a scholar whose three art names differ,
/// so a box that drops or swaps one shows in a record compared whole; the art folders with its pictures; and the draft
/// holding the scholar.
/// </summary>
internal static class PartyMemberFixtures
{
    /// <summary>A member of <see cref="ClassFixtures.Scholar"/>'s class.</summary>
    public static PartyMemberDefinition Scholar { get; } = new()
    {
        Id = "ilsa",
        Name = "Ilsa",
        ClassId = ClassFixtures.Scholar.Id,
        Summary = "A reader who left the library to see what the books got wrong.",
        PortraitFile = "IlsaPortrait.png",
        BattleSpriteFile = "IlsaBattler.png",
        AttackEffectFile = "InkSplash.png",
    };

    /// <summary>
    /// The art folders as the shell's lookup reads them, each holding the scholar's picture and another one.
    /// </summary>
    public static IReadOnlyList<string> ImagesIn(string folder) => folder switch
    {
        ArtFolders.Battlers => ["IlsaBattler.png", "Knight.png"],
        ArtFolders.Portraits => ["IlsaPortrait.png", "Knight.png"],
        ArtFolders.Effects => ["InkSplash.png", "Slash.png"],
        _ => [],
    };

    /// <summary>The class fixtures' draft with the scholar as its only member.</summary>
    public static ContentCatalog World { get; } = ClassFixtures.World with
    {
        PartyMembers = new Dictionary<string, PartyMemberDefinition> { [Scholar.Id] = Scholar },
    };
}
